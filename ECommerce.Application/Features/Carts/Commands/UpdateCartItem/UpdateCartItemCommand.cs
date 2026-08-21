using ECommerce.Application.DTOs.Shopping;

namespace ECommerce.Application.Features.Carts.Commands.UpdateCartItem;

public record UpdateCartItemCommand(string UserId, Guid ItemId, UpdateCartItemRequest Request) : IRequest<Result<CartDto>>;
