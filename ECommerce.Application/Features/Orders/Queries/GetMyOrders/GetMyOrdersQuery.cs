using ECommerce.Application.DTOs.Orders;

namespace ECommerce.Application.Features.Orders.Queries.GetMyOrders;

public record GetMyOrdersQuery(string UserId) : IRequest<List<OrderDto>>;
