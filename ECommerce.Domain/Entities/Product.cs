namespace ECommerce.Domain.Entities;

public class Product : BaseEntity
{
    private readonly List<ProductImage> _images = new();
    private readonly List<ProductVariant> _variants = new();
    private readonly List<ProductDiscount> _productDiscounts = new();

    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string SKU { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public decimal? CompareAtPrice { get; private set; }
    public int StockQuantity { get; private set; }

    public Guid CategoryId { get; private set; }
    public Category Category { get; private set; } = null!;

    public Guid? BrandId { get; private set; }
    public Brand? Brand { get; private set; }

    public IReadOnlyCollection<ProductImage> Images => _images;
    public IReadOnlyCollection<ProductVariant> Variants => _variants;
    public IReadOnlyCollection<ProductDiscount> ProductDiscounts => _productDiscounts;

    private Product()
    {
    }

    public static Product Create(string name, string? description, string sku, decimal price, decimal? compareAtPrice, int stockQuantity, Guid categoryId, Guid? brandId, string actorId)
    {
        ValidateDetails(name, sku, price, stockQuantity, categoryId);

        return new Product
        {
            Name = name.Trim(),
            Description = description?.Trim() ?? string.Empty,
            SKU = sku.Trim(),
            Price = price,
            CompareAtPrice = compareAtPrice,
            StockQuantity = stockQuantity,
            CategoryId = categoryId,
            BrandId = brandId,
            CreatedById = actorId
        };
    }

    public void UpdateDetails(string name, string? description, string sku, decimal price, decimal? compareAtPrice, int stockQuantity, Guid categoryId, Guid? brandId, bool isActive, string actorId)
    {
        ValidateDetails(name, sku, price, stockQuantity, categoryId);

        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
        SKU = sku.Trim();
        Price = price;
        CompareAtPrice = compareAtPrice;
        StockQuantity = stockQuantity;
        CategoryId = categoryId;
        BrandId = brandId;
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

    public ProductImage AddImage(string imageUrl, string actorId)
    {
        var isMain = !_images.Any(i => i.IsMain);
        var displayOrder = _images.Count;

        var image = ProductImage.Create(Id, imageUrl, isMain, displayOrder, actorId);
        _images.Add(image);
        return image;
    }

    public void RemoveImage(Guid imageId)
    {
        var image = _images.FirstOrDefault(i => i.Id == imageId);
        if (image is null)
        {
            throw new DomainException("ProductImage.NotFound", "This image does not exist.");
        }

        _images.Remove(image);

        if (image.IsMain)
        {
            var next = _images.OrderBy(i => i.DisplayOrder).FirstOrDefault();
            next?.MarkAsMain();
        }
    }

    public void SetMainImage(Guid imageId)
    {
        if (_images.All(i => i.Id != imageId))
        {
            throw new DomainException("ProductImage.NotFound", "This image does not exist.");
        }

        foreach (var image in _images)
        {
            if (image.Id == imageId)
            {
                image.MarkAsMain();
            }
            else
            {
                image.UnmarkAsMain();
            }
        }
    }

    public ProductVariant AddVariant(string sku, decimal? price, int stockQuantity, string? size, string? color, string actorId)
    {
        var variant = ProductVariant.Create(Id, sku, price, stockQuantity, size, color, actorId);
        _variants.Add(variant);
        return variant;
    }

    public void UpdateVariant(Guid variantId, string sku, decimal? price, int stockQuantity, string? size, string? color, bool isActive, string actorId)
    {
        var variant = _variants.FirstOrDefault(v => v.Id == variantId);
        if (variant is null)
        {
            throw new DomainException("Variant.NotFound", "This variant does not exist.");
        }

        variant.UpdateDetails(sku, price, stockQuantity, size, color, isActive, actorId);
    }

    public void RemoveVariant(Guid variantId)
    {
        var variant = _variants.FirstOrDefault(v => v.Id == variantId);
        if (variant is null)
        {
            throw new DomainException("Variant.NotFound", "This variant does not exist.");
        }

        variant.IsDeleted = true;
    }

    private static void ValidateDetails(string name, string sku, decimal price, int stockQuantity, Guid categoryId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Product.NameRequired", "A product must have a name.");
        }

        if (string.IsNullOrWhiteSpace(sku))
        {
            throw new DomainException("Product.SkuRequired", "A product must have a SKU.");
        }

        if (price < 0)
        {
            throw new DomainException("Product.NegativePrice", "The product price cannot be negative.");
        }

        if (stockQuantity < 0)
        {
            throw new DomainException("Product.NegativeStock", "The product stock cannot be negative.");
        }

        if (categoryId == Guid.Empty)
        {
            throw new DomainException("Product.MissingCategory", "A product must belong to a category.");
        }
    }
}
