namespace ECommerce.Domain.Entities;

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; private set; }
    public Order Order { get; private set; } = null!;

    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;

    public Guid? ProductVariantId { get; private set; }
    public ProductVariant? ProductVariant { get; private set; }

    public string ProductName { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public Money UnitPrice { get; private set; } = Money.Zero;
    public Money DiscountApplied { get; private set; } = Money.Zero;

    public Money LineTotalBeforeDiscount => UnitPrice * Quantity;
    public Money LineTotal => LineTotalBeforeDiscount - DiscountApplied;

    private OrderItem()
    {
    }

    internal static OrderItem Create(Guid orderId, Guid productId, Guid? productVariantId, string productName, int quantity, Money unitPrice, Money discountApplied, string actorId)
    {
        if (string.IsNullOrWhiteSpace(productName))
        {
            throw new DomainException("OrderItem.MissingProductName", "An order item must have a product name.");
        }

        if (quantity <= 0)
        {
            throw new DomainException("Order.InvalidQuantity", "Order item quantity must be greater than zero.");
        }

        return new OrderItem
        {
            OrderId = orderId,
            ProductId = productId,
            ProductVariantId = productVariantId,
            ProductName = productName,
            Quantity = quantity,
            UnitPrice = unitPrice,
            DiscountApplied = discountApplied,
            CreatedById = actorId
        };
    }
}
