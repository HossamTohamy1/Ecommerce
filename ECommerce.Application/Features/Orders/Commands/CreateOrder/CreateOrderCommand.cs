using ECommerce.Application.DTOs.Orders;

namespace ECommerce.Application.Features.Orders.Commands.CreateOrder;

public record CreateOrderCommand(string UserId, CreateOrderRequest Request) : IRequest<Result<OrderDto>>;
