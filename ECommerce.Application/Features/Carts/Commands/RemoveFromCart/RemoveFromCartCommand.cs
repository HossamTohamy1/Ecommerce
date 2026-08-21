using ECommerce.Application.DTOs.Shopping;

namespace ECommerce.Application.Features.Carts.Commands.RemoveFromCart;

public record RemoveFromCartCommand(string UserId, Guid ItemId) : IRequest<Result<CartDto>>;
