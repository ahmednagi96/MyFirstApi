using System.ComponentModel.DataAnnotations;

namespace MyFirstApi.DTOs.Auth;

public record ForgotPasswordDto(
    [Required, EmailAddress]
    string Email
);
