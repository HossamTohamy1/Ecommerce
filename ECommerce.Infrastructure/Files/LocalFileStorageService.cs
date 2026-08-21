using ECommerce.Shared.Results;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace ECommerce.Infrastructure.Files;

public class LocalFileStorageService : IFileStorageService
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".svg"
    };

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/jpg", "image/pjpeg", "image/png", "image/x-png", "image/webp", "image/svg+xml", "image/svg", "application/octet-stream"
    };

    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10MB

    private readonly IWebHostEnvironment _environment;
    private readonly IStringLocalizer<SharedResource> _localizer;
    private readonly ILogger<LocalFileStorageService> _logger;

    public LocalFileStorageService(
        IWebHostEnvironment environment,
        IStringLocalizer<SharedResource> localizer,
        ILogger<LocalFileStorageService> logger)
    {
        _environment = environment;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task<Result<FileUploadResult>> SaveAsync(IFormFile file, string folder, CancellationToken ct = default)
    {
        if (file is null || file.Length == 0)
        {
            return Result<FileUploadResult>.Failure(_localizer["Files.NoFileProvided"].Value);
        }

        if (file.Length > MaxFileSizeBytes)
        {
            return Result<FileUploadResult>.Failure(_localizer["Files.TooLarge"].Value);
        }

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
        {
            return Result<FileUploadResult>.Failure(_localizer["Files.InvalidExtension"].Value);
        }

        var contentType = file.ContentType?.Split(';')[0].Trim().ToLowerInvariant();
        if (!string.IsNullOrEmpty(contentType) && !AllowedContentTypes.Contains(contentType))
        {
            return Result<FileUploadResult>.Failure(_localizer["Files.InvalidContentType"].Value);
        }

        try
        {
            var webRoot = string.IsNullOrEmpty(_environment.WebRootPath)
                ? Path.Combine(_environment.ContentRootPath, "wwwroot")
                : _environment.WebRootPath;

            var uploadsFolder = Path.Combine(webRoot, "uploads", folder);
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
            var fullPath = Path.Combine(uploadsFolder, uniqueFileName);

            await using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream, ct);
            }

            var relativeUrl = $"/uploads/{folder}/{uniqueFileName}";
            _logger.LogInformation("Saved uploaded file to {Path}", relativeUrl);

            return Result<FileUploadResult>.Success(new FileUploadResult(relativeUrl, uniqueFileName));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save uploaded file to folder {Folder}", folder);
            return Result<FileUploadResult>.Failure(_localizer["Common.UnexpectedError"].Value);
        }
    }

    public Task DeleteAsync(string relativeUrl, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(relativeUrl))
        {
            return Task.CompletedTask;
        }

        var webRoot = string.IsNullOrEmpty(_environment.WebRootPath)
            ? Path.Combine(_environment.ContentRootPath, "wwwroot")
            : _environment.WebRootPath;

        var relativePath = relativeUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.Combine(webRoot, relativePath);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            _logger.LogInformation("Deleted uploaded file at {Path}", relativeUrl);
        }

        return Task.CompletedTask;
    }
}
