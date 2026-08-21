using ECommerce.Application.DTOs.Orders;

namespace ECommerce.Application.Features.Orders.Queries.GetAllOrdersPaged;

public record GetAllOrdersPagedQuery(int Page, int PageSize) : IRequest<PagedResult<OrderDto>>;
