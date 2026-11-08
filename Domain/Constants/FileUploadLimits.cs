namespace MyFirstApi.Domain.Constants;

public static class FileUploadLimits
{
    public const long MaxFileSizeBytes = 5 * 1024 * 1024;
    public const long MaxRequestSizeBytes = MaxFileSizeBytes + (64 * 1024);

    public static readonly string[] AllowedExtensions =
    [
        ".jpg",
        ".jpeg",
        ".png",
        ".pdf",
        ".docx",
        ".svg"
    ];
}
