using System.ComponentModel.DataAnnotations;

namespace MyFirstApi.DTOs.Auth;

public record RegisterDto(
    [Required, StringLength(100, MinimumLength = 2)]
    string Name,

    [Required, EmailAddress, StringLength(256)]
    string Email,

    [Required, MinLength(8), MaxLength(128)]
    [ RegularExpression(
        @"^(?=.*[A-Za-z])(?=.*\d).{8,}$",
        ErrorMessage = "كلمة المرور 8 أحرف على الأقل وتشمل حرف ورقم.")]
    string Password
);
