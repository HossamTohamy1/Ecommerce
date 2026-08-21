using ECommerce.Application.DTOs.Catalog;

namespace ECommerce.Application.Features.Brands.Commands.UploadBrandLogo;

public record UploadBrandLogoCommand(Guid Id, IFormFile Logo, string UserId) : IRequest<Result<BrandDto>>;
