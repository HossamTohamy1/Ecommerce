namespace ECommerce.Domain.Entities;

public class WishlistItem : BaseEntity
{
    public Guid WishlistId { get; private set; }
    public Wishlist Wishlist { get; private set; } = null!;

    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;

    private WishlistItem()
    {
    }

    internal static WishlistItem Create(Guid wishlistId, Guid productId, string actorId)
    {
        return new WishlistItem
        {
            WishlistId = wishlistId,
            ProductId = productId,
            CreatedById = actorId
        };
    }
}
