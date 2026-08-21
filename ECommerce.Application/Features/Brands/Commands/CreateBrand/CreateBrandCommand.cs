using ECommerce.Application.DTOs.Catalog;

namespace ECommerce.Application.Features.Brands.Commands.CreateBrand;

public record CreateBrandCommand(CreateBrandRequest Request, string UserId) : IRequest<Result<BrandDto>>;
