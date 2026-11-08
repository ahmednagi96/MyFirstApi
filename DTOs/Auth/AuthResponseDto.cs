namespace MyFirstApi.DTOs.Auth;

public record AuthResponseDto(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresOn,
    DateTime RefreshTokenExpiresOn,
    AuthUserDto User,
    string TokenType = "Bearer"
);
