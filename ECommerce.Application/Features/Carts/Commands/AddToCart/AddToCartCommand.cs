using ECommerce.Application.DTOs.Shopping;

namespace ECommerce.Application.Features.Carts.Commands.AddToCart;

public record AddToCartCommand(string UserId, AddCartItemRequest Request) : IRequest<Result<CartDto>>;
