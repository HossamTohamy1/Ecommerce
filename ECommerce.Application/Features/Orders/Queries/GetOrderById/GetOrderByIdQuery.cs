using ECommerce.Application.DTOs.Orders;

namespace ECommerce.Application.Features.Orders.Queries.GetOrderById;

public record GetOrderByIdQuery(Guid Id, string UserId, bool IsAdmin) : IRequest<Result<OrderDto>>;
