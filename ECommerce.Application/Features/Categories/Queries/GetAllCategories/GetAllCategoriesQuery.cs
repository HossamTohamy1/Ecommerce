using ECommerce.Application.DTOs.Catalog;

namespace ECommerce.Application.Features.Categories.Queries.GetAllCategories;

public record GetAllCategoriesQuery : IRequest<List<CategoryDto>>;
