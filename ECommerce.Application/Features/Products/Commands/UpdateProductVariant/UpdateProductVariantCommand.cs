using ECommerce.Application.DTOs.Catalog;

namespace ECommerce.Application.Features.Products.Commands.UpdateProductVariant;

public record UpdateProductVariantCommand(Guid ProductId, Guid VariantId, UpdateProductVariantRequest Request, string UserId) : IRequest<Result<ProductVariantDto>>;
