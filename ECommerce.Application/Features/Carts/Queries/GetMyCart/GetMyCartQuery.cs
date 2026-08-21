using ECommerce.Application.DTOs.Shopping;

namespace ECommerce.Application.Features.Carts.Queries.GetMyCart;

public record GetMyCartQuery(string UserId) : IRequest<CartDto>;
