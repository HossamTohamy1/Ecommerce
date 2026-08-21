namespace ECommerce.Domain.Entities;

public class ProductVariant : BaseEntity
{
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;

    public string SKU { get; private set; } = string.Empty;
    public decimal? Price { get; private set; }
    public int StockQuantity { get; private set; }

    public string? Size { get; private set; }
    public string? Color { get; private set; }

    private ProductVariant()
    {
    }

    internal static ProductVariant Create(Guid productId, string sku, decimal? price, int stockQuantity, string? size, string? color, string actorId)
    {
        ValidateDetails(sku, stockQuantity);

        return new ProductVariant
        {
            ProductId = productId,
            SKU = sku.Trim(),
            Price = price,
            StockQuantity = stockQuantity,
            Size = size,
            Color = color,
            CreatedById = actorId
        };
    }

    public void UpdateDetails(string sku, decimal? price, int stockQuantity, string? size, string? color, bool isActive, string actorId)
    {
        ValidateDetails(sku, stockQuantity);

        SKU = sku.Trim();
        Price = price;
        StockQuantity = stockQuantity;
        Size = size;
        Color = color;
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;
        UpdatedById = actorId;
    }

    public void DecreaseStock(int quantity)
    {
        if (quantity <= 0)
        {
            throw new DomainException("Product.InvalidQuantity", "Quantity must be greater than zero.");
        }

        if (StockQuantity < quantity)
        {
            throw new DomainException("Product.InsufficientStock", "Not enough stock available.");
        }

        StockQuantity -= quantity;
    }

    public void IncreaseStock(int quantity)
    {
        if (quantity <= 0)
        {
            throw new DomainException("Product.InvalidQuantity", "Quantity must be greater than zero.");
        }

        StockQuantity += quantity;
    }

    private static void ValidateDetails(string sku, int stockQuantity)
    {
        if (string.IsNullOrWhiteSpace(sku))
        {
            throw new DomainException("Variant.SkuRequired", "A variant must have a SKU.");
        }

        if (stockQuantity < 0)
        {
            throw new DomainException("Product.NegativeStock", "The stock cannot be negative.");
        }
    }
}
