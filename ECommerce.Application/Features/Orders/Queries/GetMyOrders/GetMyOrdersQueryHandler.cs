using ECommerce.Application.DTOs.Orders;

namespace ECommerce.Application.Features.Orders.Queries.GetMyOrders;

public class GetMyOrdersQueryHandler : IRequestHandler<GetMyOrdersQuery, List<OrderDto>>
{
    private readonly IApplicationDbContext _context;

    public GetMyOrdersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<OrderDto>> Handle(GetMyOrdersQuery request, CancellationToken ct)
    {
        return await _context.Set<Order>()
            .AsNoTracking()
            .Where(o => o.UserId == request.UserId)
            .OrderByDescending(o => o.CreatedAt)
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
    }
}
