namespace MyFirstApi.Services;

public interface ICurrentUser
{
    bool IsAuthenticated { get; }
    int? UserId { get; }
    string? Email { get; }
    string? Role { get; }
    int GetRequiredUserId();
}
