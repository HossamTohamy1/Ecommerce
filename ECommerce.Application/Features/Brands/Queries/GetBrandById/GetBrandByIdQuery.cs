using ECommerce.Application.DTOs.Catalog;

namespace ECommerce.Application.Features.Brands.Queries.GetBrandById;

public record GetBrandByIdQuery(Guid Id) : IRequest<Result<BrandDto>>;
