using ECommerce.Application.DTOs.Catalog;

namespace ECommerce.Application.Features.Categories.Commands.CreateCategory;

public record CreateCategoryCommand(CreateCategoryRequest Request, string UserId) : IRequest<Result<CategoryDto>>;
