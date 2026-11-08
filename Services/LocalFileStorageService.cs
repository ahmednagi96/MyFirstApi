using Microsoft.AspNetCore.Mvc;

namespace MyFirstApi.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _targetFolderPath;

    public LocalFileStorageService(IWebHostEnvironment env)
    {
        var webRootPath = env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        _targetFolderPath = Path.Combine(webRootPath, "uploads");

        if (!Directory.Exists(_targetFolderPath))
            Directory.CreateDirectory(_targetFolderPath);
    }

    public async Task<string> UploadFileAsync(
        Stream content,
        string fileName,
        string contentType,
        CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var fullFilePath = Path.Combine(_targetFolderPath, uniqueFileName);

        await using var fileStream = new FileStream(
            fullFilePath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 81920,
            useAsync: true);

        await content.CopyToAsync(fileStream, cancellationToken);

        return $"/uploads/{uniqueFileName}";
    }

}
