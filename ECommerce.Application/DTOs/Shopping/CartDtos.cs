
namespace ECommerce.Application.DTOs.Shopping;

public class CartItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public Guid? ProductVariantId { get; set; }
    public string? VariantLabel { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal => UnitPrice * Quantity;
}

public class CartDto
{
    public Guid Id { get; set; }
    public List<CartItemDto> Items { get; set; } = new();
    public decimal Total => Items.Sum(i => i.LineTotal);
}

public class AddCartItemRequest
{
    [Required]
    public Guid ProductId { get; set; }

    public Guid? ProductVariantId { get; set; }

    [Range(1, 1000)]
    public int Quantity { get; set; } = 1;
}

public class UpdateCartItemRequest
{
    [Range(1, 1000)]
    public int Quantity { get; set; }
}
