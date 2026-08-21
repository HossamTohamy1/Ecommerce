namespace ECommerce.Domain.Entities;

public class Cart : BaseEntity
{
    private readonly List<CartItem> _items = new();

    public string? UserId { get; private set; }
    public string? SessionId { get; private set; }

    public IReadOnlyCollection<CartItem> Items => _items;

    private Cart()
    {
    }

    public static Cart Create(string? userId, string? sessionId, string actorId)
    {
        if (string.IsNullOrWhiteSpace(userId) && string.IsNullOrWhiteSpace(sessionId))
        {
            throw new DomainException("Cart.MissingOwner", "A cart must belong to either a user or a session.");
        }

        return new Cart
        {
            UserId = userId,
            SessionId = sessionId,
            CreatedById = actorId ?? string.Empty
        };
    }

    public void AddOrMergeItem(Guid productId, Guid? productVariantId, int quantity, Money unitPrice, string actorId)
    {
        if (quantity <= 0)
        {
            throw new DomainException("Cart.InvalidQuantity", "Cart item quantity must be greater than zero.");
        }

        var existing = _items.FirstOrDefault(i => i.ProductId == productId && i.ProductVariantId == productVariantId);
        if (existing is not null)
        {
            existing.ChangeQuantity(existing.Quantity + quantity);
            existing.ChangePrice(unitPrice);
            return;
        }

        _items.Add(CartItem.Create(Id, productId, productVariantId, quantity, unitPrice, actorId));
    }

    public void UpdateItemQuantity(Guid itemId, int quantity)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item is null)
        {
            throw new DomainException("Cart.ItemNotFound", "This cart item does not exist.");
        }

        item.ChangeQuantity(quantity);
    }

    public void RemoveItem(Guid itemId)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item is null)
        {
            throw new DomainException("Cart.ItemNotFound", "This cart item does not exist.");
        }

        _items.Remove(item);
    }

    public void Clear()
    {
        _items.Clear();
    }
}
