using Humanizer;
using MyFirstApi.DTOs.Users.Responses;

namespace MyFirstApi.DTOs.Posts.Responses;

public record PostDetailsDto(
    int Id,
    string Title,
    string Content,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    UserResponseDto? Author
    )
{
    public string TimeAgo => CreatedAt.Humanize();
}
