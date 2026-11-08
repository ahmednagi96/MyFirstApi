using System.ComponentModel.DataAnnotations;
using MyFirstApi.Enums;

namespace MyFirstApi.DTOs.Categories.Requests;

public class CreateCategoryRequestDto
{

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;

    [Required]
    [StringLength(255, MinimumLength = 2)]
    public string Description { get; init; } = string.Empty;

    [Required]
    [EnumDataType(typeof(CategoryType))]
    public CategoryType Type { get; init; } = CategoryType.All;

    [Required]
    public bool IsActive { get; init; } = true;
    
}
