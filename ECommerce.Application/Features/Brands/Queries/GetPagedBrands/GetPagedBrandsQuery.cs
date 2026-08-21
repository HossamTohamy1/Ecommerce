using ECommerce.Application.DTOs.Catalog;

namespace ECommerce.Application.Features.Brands.Queries.GetPagedBrands;

public record GetPagedBrandsQuery(int Page, int PageSize) : IRequest<PagedResult<BrandDto>>;
