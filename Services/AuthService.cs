using System.Security.Cryptography;
using MediatR;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MyFirstApi.Configuration;
using MyFirstApi.Data;
using MyFirstApi.DTOs.Auth;
using MyFirstApi.Entities;
using MyFirstApi.Exceptions;
using MyFirstApi.Events;

namespace MyFirstApi.Services;

public class AuthService(
    IPublisher publisher,
    AppDbContext context,
    ITokenService tokenService,
    IPasswordHasher<User> passwordHasher,
  //  IBackgroundJobClient backgroundJobClient,
    IOptions<JwtSettings> jwtOptions) : IAuthService
{
    private readonly JwtSettings _jwt = jwtOptions.Value;

    public async Task<RegisterResponseDto> RegisterAsync(RegisterDto model, CancellationToken ct = default)
    {
        var email = NormalizeEmail(model.Email);

        if (await context.Users.AnyAsync(u => u.Email == email, ct))
            throw new ConflictException("البريد الإلكتروني مستخدم بالفعل.");

        var user = new User
        {
            Name = model.Name.Trim(),
            Email = email,
            Status = "Active"
        };
        user.PasswordHash = passwordHasher.HashPassword(user, model.Password);
        context.Users.Add(user);
        await context.SaveChangesAsync(ct);

        await AssignDefaultUserRoleAsync(user.Id, ct);

        return new RegisterResponseDto(user.Id, "تم إنشاء الحساب بنجاح. يمكنك تسجيل الدخول الآن.");
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto model, string? ipAddress, CancellationToken ct = default)
    {
        var email = NormalizeEmail(model.Email);
        var user = await context.Users.SingleOrDefaultAsync(u => u.Email == email, ct);

        if (user is null || user.DeletedAt is not null || string.IsNullOrEmpty(user.PasswordHash))
            throw new UnauthorizedAccessException("البريد الإلكتروني أو كلمة المرور غير صحيحة.");

        var verification = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);
        if (verification == PasswordVerificationResult.Failed)
            throw new UnauthorizedAccessException("البريد الإلكتروني أو كلمة المرور غير صحيحة.");

        if (!string.Equals(user.Status, "Active", StringComparison.OrdinalIgnoreCase))
            throw new ForbiddenException("الحساب غير نشط.");

        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
            user.PasswordHash = passwordHasher.HashPassword(user, model.Password);

        var session = await CreateSessionAsync(user, ipAddress, ct);
        await context.SaveChangesAsync(ct);
        return session;
    }

    public async Task<AuthResponseDto> LoginWithExternalAsync(
        ExternalLoginDto login,
        string? ipAddress,
        CancellationToken ct = default)
    {
        var email = NormalizeEmail(login.Email);
        var provider = login.Provider.Trim();
        var providerKey = login.ProviderKey.Trim();

        var userLogin = await context.UserLogins
            .Include(ul => ul.User)
            .SingleOrDefaultAsync(
                ul => ul.LoginProvider == provider && ul.ProviderKey == providerKey,
                ct);

        User user;
        if (userLogin is not null)
        {
            user = userLogin.User;
        }
        else
        {
            user = await context.Users.SingleOrDefaultAsync(u => u.Email == email, ct)
                ?? CreateExternalUser(email, login.Name);

            if (user.Id == 0)
            {
                await context.SaveChangesAsync(ct);
                await AssignDefaultUserRoleAsync(user.Id, ct);
            }

            context.UserLogins.Add(new UserLogin
            {
                LoginProvider = provider,
                ProviderKey = providerKey,
                ProviderDisplayName = provider,
                UserId = user.Id
            });
        }

        if (user.DeletedAt is not null
            || !string.Equals(user.Status, "Active", StringComparison.OrdinalIgnoreCase))
            throw new ForbiddenException("الحساب غير نشط.");

        var session = await CreateSessionAsync(user, ipAddress, ct);
        await context.SaveChangesAsync(ct);
        return session;
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken, string? ipAddress, CancellationToken ct = default)
    {
        var tokenHash = tokenService.HashToken(refreshToken);
        var stored = await context.RefreshTokens
            .Include(rt => rt.User)
            .SingleOrDefaultAsync(rt => rt.TokenHash == tokenHash, ct);

        if (stored is null)
            throw new UnauthorizedAccessException("جلسة العمل غير صالحة أو منتهية الصلاحية.");

        if (!stored.IsActive)
        {
            await RevokeAllUserTokensAsync(stored.UserId, ct);
            await context.SaveChangesAsync(ct);
            throw new UnauthorizedAccessException("تم اكتشاف إعادة استخدام توكن قديم. تم إنهاء كل الجلسات.");
        }

        if (stored.User.DeletedAt is not null
            || !string.Equals(stored.User.Status, "Active", StringComparison.OrdinalIgnoreCase))
                 throw new ForbiddenException("الحساب غير نشط.");

        stored.RevokedOn = DateTime.UtcNow;
        var session = await CreateSessionAsync(stored.User, ipAddress, ct);
        stored.ReplacedByTokenHash = tokenService.HashToken(session.RefreshToken);
        await context.SaveChangesAsync(ct);
        return session;
    }

    public async Task RevokeTokenAsync(string refreshToken, CancellationToken ct = default)
    {
// الداله دي بتعمل تشفير لل rawtoken عشان يبقي زي ال refreshtoken والمقارنه تطلع صح ويعدي وهي بردو نفس الداله اللي اتنشأ منها يعمني مسئوله عن الانشاء والمقارنه  
        var tokenHash = tokenService.HashToken(refreshToken);
        var stored = await context.RefreshTokens
            .SingleOrDefaultAsync(rt => rt.TokenHash == tokenHash, ct);

        if (stored is null || !stored.IsActive)
            return;

        stored.RevokedOn = DateTime.UtcNow;
        await context.SaveChangesAsync(ct);
    }

    public async Task RevokeAllSessionsAsync(int userId, CancellationToken ct = default)
    {
        await RevokeAllUserTokensAsync(userId, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task ForgotPasswordAsync(string email, CancellationToken ct = default)
    {
        var normalized = NormalizeEmail(email);
        var user = await context.Users.SingleOrDefaultAsync(
            u => u.Email == normalized && u.DeletedAt == null,
            ct);

        if (user is null)
            return;

        var rawToken = tokenService.CreateRefreshToken().RawToken;
        user.PasswordResetTokenHash = tokenService.HashToken(rawToken);
        user.PasswordResetExpiresOn = DateTime.UtcNow.AddMinutes(_jwt.PasswordResetExpirationMinutes);
        user.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(ct);
            string emailBody = $@"
                <div style='font-family: Arial, sans-serif; padding: 20px;'>
                <h2>إعادة تعيين كلمة المرور</h2>
                <p>استخدم التوكن التالي خلال {_jwt.PasswordResetExpirationMinutes} دقيقة:</p>
                <h3 style='color: #2b6cb0;'>{rawToken}</h3>
            </div>";
 //   backgroundJobClient.Enqueue<IEmailService>(
   //      x => x.SendEmailAsync(user.Email, "إعادة تعيين كلمة المرور", emailBody, CancellationToken.None));    }
        await publisher.Publish(new PasswordResetRequestedEvent(user.Id, user.Name, user.Email, "إعادة تعيين كلمة المرور", emailBody));
    }
    public async Task ResetPasswordAsync(ResetPasswordDto model, CancellationToken ct = default)
    {
        var email = NormalizeEmail(model.Email);
        var user = await context.Users.SingleOrDefaultAsync(u => u.Email == email, ct);
        var tokenHash = tokenService.HashToken(model.Token);

        var tokenValid = user is not null
            && user.DeletedAt is null
            && user.PasswordResetTokenHash is not null
            && user.PasswordResetExpiresOn is not null
            && user.PasswordResetExpiresOn > DateTime.UtcNow
            && CryptographicEquals(user.PasswordResetTokenHash, tokenHash);

        if (!tokenValid || user is null)
            throw new ArgumentException("رابط إعادة التعيين غير صالح أو منتهي.");

        user.PasswordHash = passwordHasher.HashPassword(user, model.NewPassword);
        user.PasswordResetTokenHash = null;
        user.PasswordResetExpiresOn = null;
        user.UpdatedAt = DateTime.UtcNow;

        await RevokeAllUserTokensAsync(user.Id, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task ChangePasswordAsync(int userId, ChangePasswordDto model, CancellationToken ct = default)
    {
        var user = await context.Users.SingleOrDefaultAsync(
            u => u.Id == userId && u.DeletedAt == null,
            ct) ?? throw new KeyNotFoundException("المستخدم غير موجود.");

        if (string.IsNullOrEmpty(user.PasswordHash)
            || passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.CurrentPassword)
                == PasswordVerificationResult.Failed)
            throw new UnauthorizedAccessException("كلمة المرور الحالية غير صحيحة.");

        user.PasswordHash = passwordHasher.HashPassword(user, model.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await RevokeAllUserTokensAsync(user.Id, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task<AuthUserDto> GetMeAsync(int userId, CancellationToken ct = default)
    {
        var user = await context.Users.AsNoTracking()
            .SingleOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null, ct)
            ?? throw new KeyNotFoundException("المستخدم غير موجود.");

        var authorization = await GetUserAuthorizationAsync(user.Id, ct);
        return ToAuthUser(user, authorization.Roles, authorization.Permissions);
    }

    private User CreateExternalUser(string email, string? name)
    {
        var user = new User
        {
            Name = string.IsNullOrWhiteSpace(name) ? email : name.Trim(),
            Email = email,
            Status = "Active",
            PasswordHash = null
        };

        context.Users.Add(user);
        return user;
    }

    private async Task<AuthResponseDto> CreateSessionAsync(
        User user, string? ipAddress, CancellationToken ct)
    {
        var authorization = await GetUserAuthorizationAsync(user.Id, ct);
        var accessToken = tokenService.CreateAccessToken(
            user, authorization.Roles, authorization.Permissions);
        var accessExpires = tokenService.GetAccessTokenExpiresOn();
        var (rawRefresh, refreshHash, refreshExpires) = tokenService.CreateRefreshToken();

        context.RefreshTokens.Add(new RefreshToken
        {
            TokenHash = refreshHash,
            ExpiresOn = refreshExpires,
            CreatedByIp = ipAddress,
            UserId = user.Id
        });

        return new AuthResponseDto(
            accessToken,
            rawRefresh,
            accessExpires,
            refreshExpires,
            ToAuthUser(user, authorization.Roles, authorization.Permissions));
    }

    private async Task AssignDefaultUserRoleAsync(int userId, CancellationToken ct)
    {
        var defaultRole = await context.Roles.SingleOrDefaultAsync(r => r.Name == "User", ct);
        if (defaultRole is null)
            return;

        context.UserRoles.Add(new UserRole
        {
            UserId = userId,
            RoleId = defaultRole.Id
        });
        await context.SaveChangesAsync(ct);
    }

    private async Task<(IReadOnlyList<string> Roles, IReadOnlyList<string> Permissions)>
        GetUserAuthorizationAsync(int userId, CancellationToken ct)
       {
            var userRoles = await context.UserRoles
                .AsNoTracking()
                .Where(ur => ur.UserId == userId)
                .Select(ur => new { ur.RoleId, RoleName = ur.Role.Name })
                .ToListAsync(ct);

            var roles = userRoles
                .Select(r => r.RoleName)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var roleIds = userRoles.Select(r => r.RoleId).Distinct().ToList();
            var permissions = roleIds.Count == 0
                ? []
                : await context.RolePermissions
                    .AsNoTracking()
                    .Where(rp => roleIds.Contains(rp.RoleId))
                    .Select(rp => rp.Permission.Name)
                    .Where(name => name != null && name != "")
                    .Distinct()
                    .ToListAsync(ct);

            return (roles, permissions);
    }

    private async Task RevokeAllUserTokensAsync(int userId, CancellationToken ct)
    {
        var activeTokens = await context.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedOn == null && rt.ExpiresOn > DateTime.UtcNow)
            .ToListAsync(ct);

        var now = DateTime.UtcNow;
        foreach (var token in activeTokens)
            token.RevokedOn = now;
    }

    private static AuthUserDto ToAuthUser(
        User user,
        IReadOnlyList<string> roles,
        IReadOnlyList<string> permissions)
        => new(user.Id, user.Name, user.Email, roles, permissions);

    private static string NormalizeEmail(string email)
        => email.Trim().ToLowerInvariant();

    private static bool CryptographicEquals(string left, string right)
    {
        var leftBytes = Encoding.UTF8.GetBytes(left);
        var rightBytes = Encoding.UTF8.GetBytes(right);
        return leftBytes.Length == rightBytes.Length
            && CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
    }
}
