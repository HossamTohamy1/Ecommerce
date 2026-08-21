using ECommerce.Application.DTOs.Orders;

namespace ECommerce.Application.Features.Orders.Queries.GetMyOrdersPaged;

public record GetMyOrdersPagedQuery(string UserId, int Page, int PageSize) : IRequest<PagedResult<OrderDto>>;
