
namespace ECommerce.Application.DTOs.Catalog;

public class CreateProductVariantRequest
{
    [Required]
    [StringLength(50)]
    public string SKU { get; set; } = string.Empty;

    public decimal? Price { get; set; }

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }

    [StringLength(50)]
    public string? Size { get; set; }

    [StringLength(50)]
    public string? Color { get; set; }
}

public class UpdateProductVariantRequest : CreateProductVariantRequest
{
    public bool IsActive { get; set; } = true;
}
