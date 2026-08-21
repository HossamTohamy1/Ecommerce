namespace ECommerce.Domain.Entities;

public class ProductDiscount : BaseEntity
{
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;

    public Guid DiscountId { get; private set; }
    public Discount Discount { get; private set; } = null!;

    private ProductDiscount()
    {
    }

    internal static ProductDiscount Create(Guid discountId, Guid productId, string actorId)
    {
        return new ProductDiscount
        {
            DiscountId = discountId,
            ProductId = productId,
            CreatedById = actorId
        };
    }
}
