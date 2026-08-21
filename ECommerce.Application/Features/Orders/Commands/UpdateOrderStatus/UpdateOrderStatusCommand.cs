using ECommerce.Application.DTOs.Orders;

namespace ECommerce.Application.Features.Orders.Commands.UpdateOrderStatus;

public record UpdateOrderStatusCommand(Guid Id, UpdateOrderStatusRequest Request, string AdminUserId) : IRequest<Result<OrderDto>>;
