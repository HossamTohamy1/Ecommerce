using ECommerce.Application.DTOs.Catalog;

namespace ECommerce.Application.Features.Categories.Queries.GetPagedCategories;

public record GetPagedCategoriesQuery(int Page, int PageSize) : IRequest<PagedResult<CategoryDto>>;
