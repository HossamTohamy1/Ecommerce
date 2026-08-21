namespace ECommerce.Domain.Entities;

public class CartItem : BaseEntity
{
    public Guid CartId { get; private set; }
    public Cart Cart { get; private set; } = null!;

    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;

    public Guid? ProductVariantId { get; private set; }
    public ProductVariant? ProductVariant { get; private set; }

    public int Quantity { get; private set; }
    public Money UnitPrice { get; private set; } = Money.Zero;

    public Money LineTotal => UnitPrice * Quantity;

    private CartItem()
    {
    }

    internal static CartItem Create(Guid cartId, Guid productId, Guid? productVariantId, int quantity, Money unitPrice, string actorId)
    {
        if (quantity <= 0)
        {
            throw new DomainException("Cart.InvalidQuantity", "Cart item quantity must be greater than zero.");
        }

        return new CartItem
        {
            CartId = cartId,
            ProductId = productId,
            ProductVariantId = productVariantId,
            Quantity = quantity,
            UnitPrice = unitPrice,
            CreatedById = actorId
        };
    }

    internal void ChangeQuantity(int quantity)
    {
        if (quantity <= 0)
        {
            throw new DomainException("Cart.InvalidQuantity", "Cart item quantity must be greater than zero.");
        }

        Quantity = quantity;
    }

    internal void ChangePrice(Money unitPrice)
    {
        UnitPrice = unitPrice;
    }
}
