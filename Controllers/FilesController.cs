using Microsoft.AspNetCore.Mvc;
using MyFirstApi.Domain.Constants;
using MyFirstApi.DTOs.Files.Requests;
using MyFirstApi.DTOs.Files.Responses;
using MyFirstApi.Services;

namespace MyFirstApi.Controllers;


[Route("api/[controller]")]
[Produces("application/json")]
public class FilesController(IFileStorageService fileStorageService, IConfiguration configuration) : ApiControllerBase
{
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(FileUploadLimits.MaxRequestSizeBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = FileUploadLimits.MaxRequestSizeBytes)]
    [ProducesResponseType(typeof(FileUploadResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FileUploadResponseDto>> UploadFile(
        [FromForm] UploadFileRequestDto request,
        CancellationToken cancellationToken)
    {
        await using var stream = request.File.OpenReadStream();
        var fileUrl = await fileStorageService.UploadFileAsync(
            stream,
            request.File.FileName,
            request.File.ContentType,
            cancellationToken);

        return Ok(new FileUploadResponseDto(fileUrl, ResolveBaseUrl()));
    }

    private string ResolveBaseUrl()
    {
        var configured = configuration["FileStorage:PublicBaseUrl"];
        if (!string.IsNullOrWhiteSpace(configured))
            return configured.TrimEnd('/');

        return $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
    }
    [HttpGet("test")]
    public IActionResult GetCurrentDirectory(IWebHostEnvironment env)
    {

        return Ok( env.WebRootPath ?? "test");
    }
}
