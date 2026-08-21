
namespace ECommerce.Application.Common.Interfaces;

public record FileUploadResult(string RelativeUrl, string StoredFileName);

public interface IFileStorageService
{

    Task<Result<FileUploadResult>> SaveAsync(IFormFile file, string folder, CancellationToken ct = default);

    Task DeleteAsync(string relativeUrl, CancellationToken ct = default);
}
