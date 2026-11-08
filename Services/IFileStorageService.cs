namespace MyFirstApi.Services;

public interface IFileStorageService
{
    Task<string> UploadFileAsync(
        Stream content,
        string fileName,
        string contentType,
        CancellationToken cancellationToken);
}
