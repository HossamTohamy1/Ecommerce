namespace ECommerce.Domain.Entities;

public class ProductImage : BaseEntity
{
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;

    public string ImageUrl { get; private set; } = string.Empty;
    public bool IsMain { get; private set; }
    public int DisplayOrder { get; private set; }

    private ProductImage()
    {
    }

    internal static ProductImage Create(Guid productId, string imageUrl, bool isMain, int displayOrder, string actorId)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            throw new DomainException("ProductImage.MissingUrl", "An image must have a URL.");
        }

        return new ProductImage
        {
            ProductId = productId,
            ImageUrl = imageUrl,
            IsMain = isMain,
            DisplayOrder = displayOrder,
            CreatedById = actorId
        };
    }

    public void MarkAsMain() => IsMain = true;

    public void UnmarkAsMain() => IsMain = false;
}
