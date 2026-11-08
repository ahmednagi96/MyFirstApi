namespace MyFirstApi.DTOs.Posts.Requests;

public class CreatePostRequestDto
{
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
}
