namespace MyFirstApi.DTOs.Auth;

public record ExternalLoginDto(
    string Provider,
    string ProviderKey,
    string Email,
    string? Name);
