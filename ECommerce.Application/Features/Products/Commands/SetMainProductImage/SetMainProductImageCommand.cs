namespace ECommerce.Application.Features.Products.Commands.SetMainProductImage;

public record SetMainProductImageCommand(Guid ProductId, Guid ImageId) : IRequest<Result>;
