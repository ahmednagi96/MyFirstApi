using FluentValidation;
using MyFirstApi.Domain.Constants;

namespace MyFirstApi.DTOs.Files.Requests;

public class UploadFileRequestDtoValidator : AbstractValidator<UploadFileRequestDto>
{
    public UploadFileRequestDtoValidator()
    {
        RuleFor(x => x.File)
            .NotNull()
            .WithMessage("الملف مطلوب.");

        When(x => x.File is not null, () =>
        {
            RuleFor(x => x.File.Length)
                .GreaterThan(0)
                .WithMessage("الملف فارغ.")
                .LessThanOrEqualTo(FileUploadLimits.MaxFileSizeBytes)
                .WithMessage("حجم الملف يتجاوز الحد الأقصى المسموح به (5 ميجا).");

            RuleFor(x => x.File.FileName)
                .Must(HaveAllowedExtension)
                .WithMessage("امتداد الملف غير مسموح به لأسباب أمنية.");

            RuleFor(x => x.File)
                .Must(HaveValidSignature)
                .When(x => HaveAllowedExtension(x.File.FileName))
                .WithMessage("محتوى الملف لا يطابق امتداده.");
        });
    }

    private static bool HaveAllowedExtension(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return false;

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return FileUploadLimits.AllowedExtensions.Contains(extension);
    }

    private static bool HaveValidSignature(IFormFile file)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var stream = file.OpenReadStream();
        if (!stream.CanSeek)
            return false;

        var originalPosition = stream.Position;
        try
        {
            Span<byte> header = stackalloc byte[8];
            var bytesRead = stream.Read(header);
            return extension switch
            {
                ".jpg" or ".jpeg" => bytesRead >= 2 && header[0] == 0xFF && header[1] == 0xD8,
                ".png" => bytesRead >= 8
                    && header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47
                    && header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A,
                ".pdf" => bytesRead >= 4
                    && header[0] == 0x25 && header[1] == 0x50 && header[2] == 0x44 && header[3] == 0x46,
                ".docx" => bytesRead >= 2 && header[0] == 0x50 && header[1] == 0x4B,
                _ => false
            };
        }
        finally
        {
            stream.Position = originalPosition;
        }
    }
}
