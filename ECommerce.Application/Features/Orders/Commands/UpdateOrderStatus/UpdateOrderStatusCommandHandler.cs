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
            .Include(o => o.Items)
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

        var addressSummary = await _context.Set<Address>()
            .Where(a => a.Id == order.ShippingAddressId)
            .Select(a => a.Street + ", " + a.City + ", " + a.Governorate)
            .FirstOrDefaultAsync(ct) ?? string.Empty;

        var dto = new OrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber.Value,
            UserId = order.UserId,
            Status = order.Status,
            PaymentMethod = order.PaymentMethod,
            PaymentStatus = order.PaymentStatus,
            SubTotal = order.SubTotal.Amount,
            DiscountAmount = order.DiscountAmount.Amount,
            ShippingFee = order.ShippingFee.Amount,
            TotalAmount = order.TotalAmount.Amount,
            ShippingAddressId = order.ShippingAddressId,
            ShippingAddressSummary = addressSummary,
            ConfirmedAt = order.ConfirmedAt,
            DeliveredAt = order.DeliveredAt,
            CreatedAt = order.CreatedAt,
            Items = order.Items.Select(i => new OrderItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                ProductVariantId = i.ProductVariantId,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice.Amount,
                DiscountApplied = i.DiscountApplied.Amount
            }).ToList(),
            StatusHistory = order.StatusHistory
                .OrderBy(h => h.CreatedAt)
                .Select(h => new OrderStatusHistoryDto { Status = h.Status, Note = h.Note, CreatedAt = h.CreatedAt })
                .ToList()
        };

        return Result<OrderDto>.Success(dto);
    }

    private string LocalizeDomainError(DomainException ex) => ex.Code switch
    {
        "Order.CannotChangeFinalStatus" => _localizer["Order.CannotChangeFinalStatus"].Value,
        _ => ex.Message
    };
}
