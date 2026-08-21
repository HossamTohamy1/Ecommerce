using ECommerce.Application.DTOs.Catalog;

namespace ECommerce.Application.Features.Products.Commands.UpdateProduct;

public record UpdateProductCommand(Guid Id, UpdateProductRequest Request, string UserId) : IRequest<Result<ProductDto>>;
