using Humanizer;
using MyFirstApi.Enums;
using MyFirstApi.DTOs.Tools.Responses;

namespace MyFirstApi.DTOs.Categories.Responses;

public record CategoryDetailsDto(
    int Id,
    string Name,
    string Description,
    bool IsActive,
    CategoryType Type,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? DeletedAt,
    List<ToolDetailsDto> Tools
)
{
    public string TimeAgo => CreatedAt.Humanize();
    public string CategorySummary => $"{Name} - {Type}";
}
