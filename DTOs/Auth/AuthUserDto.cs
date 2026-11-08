namespace MyFirstApi.DTOs.Auth;

public record AuthUserDto(
    int Id,
    string Name,
    string Email,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions);
