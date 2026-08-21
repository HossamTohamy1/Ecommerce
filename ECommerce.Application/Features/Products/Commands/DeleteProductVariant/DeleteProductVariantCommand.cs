namespace ECommerce.Application.Features.Products.Commands.DeleteProductVariant;

public record DeleteProductVariantCommand(Guid ProductId, Guid VariantId) : IRequest<Result>;
