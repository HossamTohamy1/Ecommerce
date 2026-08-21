using ECommerce.Application.DTOs.Orders;

namespace ECommerce.Application.Features.Orders.Queries.GetMyOrdersPaged;

public class GetMyOrdersPagedQueryHandler : IRequestHandler<GetMyOrdersPagedQuery, PagedResult<OrderDto>>
{
    private readonly IApplicationDbContext _context;

    public GetMyOrdersPagedQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<OrderDto>> Handle(GetMyOrdersPagedQuery request, CancellationToken ct)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;

        var source = _context.Set<Order>().Where(o => o.UserId == request.UserId);
        var totalCount = await source.CountAsync(ct);

        var orders = await source
            .AsNoTracking()
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new OrderDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber.Value,
                UserId = o.UserId,
                Status = o.Status,
                PaymentMethod = o.PaymentMethod,
                PaymentStatus = o.PaymentStatus,
                SubTotal = o.SubTotal.Amount,
                DiscountAmount = o.DiscountAmount.Amount,
                ShippingFee = o.ShippingFee.Amount,
                TotalAmount = o.TotalAmount.Amount,
                ShippingAddressId = o.ShippingAddressId,
                ShippingAddressSummary = o.ShippingAddress.Street + ", " + o.ShippingAddress.City + ", " + o.ShippingAddress.Governorate,
                ConfirmedAt = o.ConfirmedAt,
                DeliveredAt = o.DeliveredAt,
                CreatedAt = o.CreatedAt,
                Items = o.Items.Select(i => new OrderItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    ProductVariantId = i.ProductVariantId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice.Amount,
                    DiscountApplied = i.DiscountApplied.Amount
                }).ToList(),
                StatusHistory = o.StatusHistory
                    .OrderBy(h => h.CreatedAt)
                    .Select(h => new OrderStatusHistoryDto { Status = h.Status, Note = h.Note, CreatedAt = h.CreatedAt })
                    .ToList()
            })
            .ToListAsync(ct);

        return new PagedResult<OrderDto>
        {
            Items = orders,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}
