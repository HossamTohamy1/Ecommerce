using ECommerce.Application.DTOs.Catalog;

namespace ECommerce.Application.Features.Products.Commands.AddProductVariant;

public record AddProductVariantCommand(Guid ProductId, CreateProductVariantRequest Request, string UserId) : IRequest<Result<ProductVariantDto>>;
