using MyFirstApi.Entities;

namespace MyFirstApi.Services;

public interface ITokenService
{
    string CreateAccessToken(
        User user,
        IReadOnlyList<string> roles,
        IReadOnlyList<string> permissions);
    DateTime GetAccessTokenExpiresOn();
    (string RawToken, string TokenHash, DateTime ExpiresOn) CreateRefreshToken();
    string HashToken(string rawToken);
}
