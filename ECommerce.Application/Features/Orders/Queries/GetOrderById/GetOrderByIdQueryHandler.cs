using ECommerce.Application.DTOs.Orders;

namespace ECommerce.Application.Features.Orders.Queries.GetOrderById;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, Result<OrderDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public GetOrderByIdQueryHandler(IApplicationDbContext context, IStringLocalizer<SharedResource> localizer)
    {
        _context = context;
        _localizer = localizer;
    }

    public async Task<Result<OrderDto>> Handle(GetOrderByIdQuery request, CancellationToken ct)
    {
        var query = _context.Set<Order>().AsNoTracking().Where(o => o.Id == request.Id);
        if (!request.IsAdmin)
        {
            query = query.Where(o => o.UserId == request.UserId);
        }

        var order = await query.Select(o => new OrderDto
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
        }).FirstOrDefaultAsync(ct);

        return order is null ? Result<OrderDto>.Failure(_localizer["Order.NotFound"].Value) : Result<OrderDto>.Success(order);
    }
}
