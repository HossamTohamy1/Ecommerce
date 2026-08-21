using ECommerce.Application.DTOs.Catalog;

namespace ECommerce.Application.Features.Categories.Commands.UpdateCategory;

public record UpdateCategoryCommand(Guid Id, UpdateCategoryRequest Request, string UserId) : IRequest<Result<CategoryDto>>;
