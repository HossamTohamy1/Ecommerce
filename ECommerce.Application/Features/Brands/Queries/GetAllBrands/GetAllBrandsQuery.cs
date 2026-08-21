using ECommerce.Application.DTOs.Catalog;

namespace ECommerce.Application.Features.Brands.Queries.GetAllBrands;

public record GetAllBrandsQuery : IRequest<List<BrandDto>>;
