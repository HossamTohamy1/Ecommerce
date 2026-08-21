using ECommerce.Application.DTOs.Orders;
using ECommerce.Application.Features.Notifications.Commands.NotifyAdmins;

namespace ECommerce.Application.Features.Orders.Commands.CreateOrder;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<OrderDto>>
{
    private static readonly Money FlatShippingFee = Money.Of(50m);

    private readonly IApplicationDbContext _context;
    private readonly IStringLocalizer<SharedResource> _localizer;
    private readonly IMediator _mediator;
    private readonly IDiscountResolver _discountResolver;

    public CreateOrderCommandHandler(
        IApplicationDbContext context,
        IStringLocalizer<SharedResource> localizer,
        IMediator mediator,
        IDiscountResolver discountResolver)
    {
        _context = context;
        _localizer = localizer;
        _mediator = mediator;
        _discountResolver = discountResolver;
    }

    public async Task<Result<OrderDto>> Handle(CreateOrderCommand command, CancellationToken ct)
    {
        var address = await _context.Set<Address>()
            .FirstOrDefaultAsync(a => a.Id == command.Request.ShippingAddressId && a.UserId == command.UserId, ct);
        if (address is null)
        {
            return Result<OrderDto>.Failure(_localizer["Address.NotFound"].Value);
        }

        var cart = await _context.Set<Cart>()
            .Include(c => c.Items).ThenInclude(i => i.Product)
            .Include(c => c.Items).ThenInclude(i => i.ProductVariant)
            .FirstOrDefaultAsync(c => c.UserId == command.UserId, ct);

        if (cart is null || cart.Items.Count == 0)
        {
            return Result<OrderDto>.Failure(_localizer["Cart.Empty"].Value);
        }

        foreach (var item in cart.Items)
        {
            var availableStock = item.ProductVariant?.StockQuantity ?? item.Product.StockQuantity;
            if (availableStock < item.Quantity)
            {
                return Result<OrderDto>.Failure(_localizer["Order.InsufficientStock", item.Product.Name, availableStock].Value);
            }
        }

        Order order;
        try
        {
            order = Order.Create(command.UserId, address.Id, command.Request.PaymentMethod, FlatShippingFee, command.UserId);

            var now = DateTime.UtcNow;
            foreach (var cartItem in cart.Items)
            {
                var unitPrice = cartItem.UnitPrice;
                var lineTotal = unitPrice * cartItem.Quantity;
                var discountAmount = await CalculateDiscountAsync(cartItem.ProductId, lineTotal, now, ct);

                order.AddItem(
                    cartItem.ProductId,
                    cartItem.ProductVariantId,
                    cartItem.Product.Name,
                    cartItem.Quantity,
                    unitPrice,
                    discountAmount,
                    command.UserId);

                if (cartItem.ProductVariant is not null)
                {
                    cartItem.ProductVariant.DecreaseStock(cartItem.Quantity);
                }
                else
                {
                    cartItem.Product.DecreaseStock(cartItem.Quantity);
                }
            }
        }
        catch (DomainException ex)
        {
            return Result<OrderDto>.Failure(LocalizeDomainError(ex));
        }

        _context.Set<Order>().Add(order);
        cart.Clear();

        await _context.SaveChangesAsync(ct);

        await _mediator.Send(new NotifyAdminsCommand(
            NotificationType.OrderCreated,
            _localizer["Notification.NewOrder.Title"].Value,
            _localizer["Notification.NewOrder.Message", order.OrderNumber.Value].Value,
            $"/Orders/Details/{order.Id}"), ct);

        var dto = await _context.Set<Order>()
            .AsNoTracking()
            .Where(o => o.Id == order.Id && o.UserId == command.UserId)
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

    private async Task<Money> CalculateDiscountAsync(Guid productId, Money lineTotal, DateTime now, CancellationToken ct)
    {
        var activeDiscount = await _discountResolver.GetActiveDiscountForProductAsync(productId, now, ct);

        if (activeDiscount is null)
        {
            return Money.Zero;
        }

        activeDiscount.RecordUsage();

        return activeDiscount.CalculateDiscountAmount(lineTotal);
    }

    private string LocalizeDomainError(DomainException ex) => ex.Code switch
    {
        "Order.CannotChangeFinalStatus" => _localizer["Order.CannotChangeFinalStatus"].Value,
        _ => ex.Message
    };
}
