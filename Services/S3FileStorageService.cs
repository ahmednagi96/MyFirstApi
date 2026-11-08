using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;

namespace MyFirstApi.Services;

public class S3FileStorageService : IFileStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;

    public S3FileStorageService(IConfiguration configuration)
    {
        var accessKey = configuration["AWS:AccessKey"];
        var secretKey = configuration["AWS:SecretKey"];
        var region = RegionEndpoint.GetBySystemName(configuration["AWS:Region"]);
        _bucketName = configuration["AWS:BucketName"]!;
        _s3Client = new AmazonS3Client(accessKey, secretKey, region);
    }

    public async Task<string> UploadFileAsync(
        Stream content,
        string fileName,
        string contentType,
        CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";

        var uploadRequest = new TransferUtilityUploadRequest
        {
            InputStream = content,
            Key = uniqueFileName,
            BucketName = _bucketName,
            ContentType = string.IsNullOrWhiteSpace(contentType)
                ? "application/octet-stream"
                : contentType
        };

        var fileTransferUtility = new TransferUtility(_s3Client);
        await fileTransferUtility.UploadAsync(uploadRequest, cancellationToken);

        return $"https://{_bucketName}.s3.amazonaws.com/{uniqueFileName}";
    }
}
