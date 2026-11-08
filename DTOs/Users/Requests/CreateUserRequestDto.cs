using System.ComponentModel.DataAnnotations;

namespace MyFirstApi.DTOs.Users.Requests;

public class CreateUserRequestDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string Role { get; init; } = string.Empty;
}
