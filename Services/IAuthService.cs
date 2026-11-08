using MyFirstApi.DTOs.Auth;

namespace MyFirstApi.Services;

public interface IAuthService
{
    Task<RegisterResponseDto> RegisterAsync(RegisterDto model, CancellationToken ct = default);
    Task<AuthResponseDto> LoginAsync(LoginDto model, string? ipAddress, CancellationToken ct = default);
    Task<AuthResponseDto> LoginWithExternalAsync(ExternalLoginDto login, string? ipAddress, CancellationToken ct = default);
    Task<AuthResponseDto> RefreshTokenAsync(string refreshToken, string? ipAddress, CancellationToken ct = default);
    Task RevokeTokenAsync(string refreshToken, CancellationToken ct = default);
    Task RevokeAllSessionsAsync(int userId, CancellationToken ct = default);
    Task ForgotPasswordAsync(string email, CancellationToken ct = default);
    Task ResetPasswordAsync(ResetPasswordDto model, CancellationToken ct = default);
    Task ChangePasswordAsync(int userId, ChangePasswordDto model, CancellationToken ct = default);
    Task<AuthUserDto> GetMeAsync(int userId, CancellationToken ct = default);
}
