namespace ECommerce.Domain.Entities;

public class Wishlist : BaseEntity
{
    private readonly List<WishlistItem> _items = new();

    public string UserId { get; private set; } = string.Empty;

    public IReadOnlyCollection<WishlistItem> Items => _items;

    private Wishlist()
    {
    }

    public static Wishlist Create(string userId, string actorId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new DomainException("Wishlist.MissingOwner", "A wishlist must belong to a user.");
        }

        return new Wishlist
        {
            UserId = userId,
            CreatedById = actorId
        };
    }

    public void AddItem(Guid productId, string actorId)
    {
        if (_items.Any(i => i.ProductId == productId))
        {
            return;
        }

        _items.Add(WishlistItem.Create(Id, productId, actorId));
    }

    public void RemoveItem(Guid productId)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item is not null)
        {
            _items.Remove(item);
        }
    }
}
