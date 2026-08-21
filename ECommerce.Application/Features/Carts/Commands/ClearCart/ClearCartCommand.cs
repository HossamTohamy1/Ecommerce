namespace ECommerce.Application.Features.Carts.Commands.ClearCart;

public record ClearCartCommand(string UserId) : IRequest<Result>;
