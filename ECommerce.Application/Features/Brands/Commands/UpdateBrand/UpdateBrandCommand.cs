using ECommerce.Application.DTOs.Catalog;

namespace ECommerce.Application.Features.Brands.Commands.UpdateBrand;

public record UpdateBrandCommand(Guid Id, UpdateBrandRequest Request, string UserId) : IRequest<Result<BrandDto>>;
