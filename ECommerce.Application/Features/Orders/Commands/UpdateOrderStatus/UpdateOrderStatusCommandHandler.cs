using ECommerce.Application.DTOs.Orders;
using ECommerce.Application.Features.Notifications.Commands.NotifyUser;

namespace ECommerce.Application.Features.Orders.Commands.UpdateOrderStatus;

public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, Result<OrderDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IStringLocalizer<SharedResource> _localizer;
    private readonly IMediator _mediator;

    public UpdateOrderStatusCommandHandler(IApplicationDbContext context, IStringLocalizer<SharedResource> localizer, IMediator mediator)
    {
        _context = context;
        _localizer = localizer;
        _mediator = mediator;
    }

    public async Task<Result<OrderDto>> Handle(UpdateOrderStatusCommand command, CancellationToken ct)
    {
        var order = await _context.Set<Order>()
            .Include(o => o.StatusHistory)
            .FirstOrDefaultAsync(o => o.Id == command.Id, ct);

        if (order is null)
        {
            return Result<OrderDto>.Failure(_localizer["Order.NotFound"].Value);
        }

        try
        {
            order.ChangeStatus(command.Request.Status, command.Request.Note, command.AdminUserId);
        }
        catch (DomainException ex)
        {
            return Result<OrderDto>.Failure(LocalizeDomainError(ex));
        }

        await _context.SaveChangesAsync(ct);

        await _mediator.Send(new NotifyUserCommand(
            order.UserId,
            NotificationType.OrderStatusChanged,
            _localizer["Notification.OrderStatusChanged.Title"].Value,
            _localizer["Notification.OrderStatusChanged.Message", order.OrderNumber.Value, _localizer["Order.Status." + order.Status].Value].Value,
            $"/Orders/Details/{order.Id}"), ct);

        var dto = await _context.Set<Order>()
            .AsNoTracking()
            .Where(o => o.Id == command.Id)
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
            .FirstOrDefaultAsync(ct);

        return dto is null ? Result<OrderDto>.Failure(_localizer["Order.NotFound"].Value) : Result<OrderDto>.Success(dto);
    }

    private string LocalizeDomainError(DomainException ex) => ex.Code switch
    {
        "Order.CannotChangeFinalStatus" => _localizer["Order.CannotChangeFinalStatus"].Value,
        _ => ex.Message
    };
}
