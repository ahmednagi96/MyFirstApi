namespace MyFirstApi.DTOs.Files.Requests;

public class UploadFileRequestDto
{
    public IFormFile File { get; set; } = default!;
}
