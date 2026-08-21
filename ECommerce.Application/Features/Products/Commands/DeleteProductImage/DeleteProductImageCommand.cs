namespace ECommerce.Application.Features.Products.Commands.DeleteProductImage;

public record DeleteProductImageCommand(Guid ProductId, Guid ImageId) : IRequest<Result>;
