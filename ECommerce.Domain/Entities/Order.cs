namespace ECommerce.Domain.Entities;

public class Order : BaseEntity
{
    private readonly List<OrderItem> _items = new();
    private readonly List<OrderStatusHistory> _statusHistory = new();

    public OrderNumber OrderNumber { get; private set; } = null!;
    public string UserId { get; private set; } = string.Empty;

    public OrderStatus Status { get; private set; }
    public PaymentMethod PaymentMethod { get; private set; }
    public PaymentStatus PaymentStatus { get; private set; }

    public Money SubTotal { get; private set; } = Money.Zero;
    public Money DiscountAmount { get; private set; } = Money.Zero;
    public Money ShippingFee { get; private set; } = Money.Zero;
    public Money TotalAmount { get; private set; } = Money.Zero;

    public Guid ShippingAddressId { get; private set; }
    public Address ShippingAddress { get; private set; } = null!;

    public DateTime? ConfirmedAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }

    public IReadOnlyCollection<OrderItem> Items => _items;
    public IReadOnlyCollection<OrderStatusHistory> StatusHistory => _statusHistory;

    private Order()
    {
    }

    public static Order Create(string userId, Guid shippingAddressId, PaymentMethod paymentMethod, Money shippingFee, string actorId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new DomainException("Order.MissingCustomer", "An order must belong to a customer.");
        }

        if (shippingAddressId == Guid.Empty)
        {
            throw new DomainException("Order.MissingShippingAddress", "An order must have a shipping address.");
        }

        var order = new Order
        {
            OrderNumber = OrderNumber.Generate(),
            UserId = userId,
            Status = OrderStatus.Pending,
            PaymentMethod = paymentMethod,
            PaymentStatus = PaymentStatus.Unpaid,
            ShippingAddressId = shippingAddressId,
            ShippingFee = shippingFee,
            CreatedById = actorId
        };

        order._statusHistory.Add(OrderStatusHistory.Create(order.Id, OrderStatus.Pending, null, actorId));

        return order;
    }

    public void AddItem(Guid productId, Guid? productVariantId, string productName, int quantity, Money unitPrice, Money discountApplied, string actorId)
    {
        if (Status != OrderStatus.Pending)
        {
            throw new DomainException("Order.CannotModifyAfterPending", "Items can only be added while the order is still pending.");
        }

        if (quantity <= 0)
        {
            throw new DomainException("Order.InvalidQuantity", "Order item quantity must be greater than zero.");
        }

        var lineTotal = unitPrice * quantity;
        if (discountApplied > lineTotal)
        {
            throw new DomainException("Order.DiscountExceedsLineTotal", "Discount applied cannot exceed the line total.");
        }

        _items.Add(OrderItem.Create(Id, productId, productVariantId, productName, quantity, unitPrice, discountApplied, actorId));

        RecalculateTotals();
    }

    public void ChangeStatus(OrderStatus newStatus, string? note, string actorId)
    {
        if (Status == OrderStatus.Cancelled || Status == OrderStatus.Delivered)
        {
            throw new DomainException("Order.CannotChangeFinalStatus", "A cancelled or delivered order can no longer change status.");
        }

        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;
        UpdatedById = actorId;

        if (newStatus == OrderStatus.Confirmed)
        {
            ConfirmedAt = DateTime.UtcNow;
        }
        else if (newStatus == OrderStatus.Delivered)
        {
            DeliveredAt = DateTime.UtcNow;
            if (PaymentMethod == PaymentMethod.Cash)
            {
                PaymentStatus = PaymentStatus.Paid;
            }
        }

        _statusHistory.Add(OrderStatusHistory.Create(Id, newStatus, note, actorId));
    }

    private void RecalculateTotals()
    {
        SubTotal = _items.Aggregate(Money.Zero, (sum, item) => sum + item.LineTotalBeforeDiscount);
        DiscountAmount = _items.Aggregate(Money.Zero, (sum, item) => sum + item.DiscountApplied);
        TotalAmount = SubTotal - DiscountAmount + ShippingFee;
    }
}

public enum OrderStatus
{
    Pending = 1,
    Confirmed = 2,
    Processing = 3,
    Shipped = 4,
    Delivered = 5,
    Cancelled = 6
}

public enum PaymentMethod
{
    Cash = 1,
    BankTransfer = 2,
    Other = 3
}

public enum PaymentStatus
{
    Unpaid = 1,
    Paid = 2,
    PartiallyPaid = 3
}
