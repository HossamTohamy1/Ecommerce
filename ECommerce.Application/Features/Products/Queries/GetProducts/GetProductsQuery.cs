using ECommerce.Application.DTOs.Catalog;

namespace ECommerce.Application.Features.Products.Queries.GetProducts;

public record GetProductsQuery(ProductListQuery Query) : IRequest<PagedResult<ProductDto>>;
