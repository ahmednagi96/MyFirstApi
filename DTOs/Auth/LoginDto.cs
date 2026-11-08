using System.ComponentModel.DataAnnotations;

namespace MyFirstApi.DTOs.Auth;

public record LoginDto(
    [Required, EmailAddress]
    string Email,

    [ Required]
    string Password
);
