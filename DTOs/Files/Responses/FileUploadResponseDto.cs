namespace MyFirstApi.DTOs.Files.Responses;

public record FileUploadResponseDto
{
    public string Url { get; }
    public string FullUrl { get; }

    public FileUploadResponseDto(string url, string baseUrl)
    {
        Url = url;
        FullUrl = IsAbsolute(url)
            ? url
            : $"{baseUrl.TrimEnd('/')}/{url.TrimStart('/')}";
    }

    private static bool IsAbsolute(string url)
        => url.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
        || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
}
