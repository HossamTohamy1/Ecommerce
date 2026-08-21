using ECommerce.Application.DTOs.Catalog;

namespace ECommerce.Application.Features.Products.Commands.CreateProduct;

public record CreateProductCommand(CreateProductRequest Request, string UserId) : IRequest<Result<ProductDto>>;
