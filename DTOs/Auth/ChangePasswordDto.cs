using System.ComponentModel.DataAnnotations;

namespace MyFirstApi.DTOs.Auth;

public record ChangePasswordDto(
    [Required]
    string CurrentPassword,

    [ Required, MinLength(8), MaxLength(128)]
    [RegularExpression(
        @"^(?=.*[A-Za-z])(?=.*\d).{8,}$",
        ErrorMessage = "كلمة المرور 8 أحرف على الأقل وتشمل حرف ورقم.")]
    string NewPassword
);
