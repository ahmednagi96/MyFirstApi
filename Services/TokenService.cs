using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MyFirstApi.Configuration;
using MyFirstApi.Entities;

namespace MyFirstApi.Services;

public class TokenService(IOptions<JwtSettings> jwtOptions) : ITokenService
{
    public const string PermissionClaimType = "Permission";

    private readonly JwtSettings _jwt = jwtOptions.Value;

    public DateTime GetAccessTokenExpiresOn()
        => DateTime.UtcNow.AddMinutes(_jwt.AccessTokenExpirationMinutes);

    public string CreateAccessToken(
        User user,
        IReadOnlyList<string> roles,
        IReadOnlyList<string> permissions)
    {
        var expires = GetAccessTokenExpiresOn();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Secret));

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.Name)
        };

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        foreach (var permission in permissions)
            claims.Add(new Claim(PermissionClaimType, permission));

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expires,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public (string RawToken, string TokenHash, DateTime ExpiresOn) CreateRefreshToken()
    {
        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        return (rawToken, HashToken(rawToken), DateTime.UtcNow.AddDays(_jwt.RefreshTokenExpirationDays));
    }

    public string HashToken(string rawToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(bytes);
    }
}
