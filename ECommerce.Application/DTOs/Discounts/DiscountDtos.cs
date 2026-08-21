
namespace ECommerce.Application.DTOs.Discounts;

public class DiscountDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public DiscountType DiscountType { get; set; }
    public decimal Value { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal? MinimumOrderAmount { get; set; }
    public int? UsageLimit { get; set; }
    public int UsageCount { get; set; }
    public bool IsActive { get; set; }
    public List<Guid> ProductIds { get; set; } = new();
}

public class CreateDiscountRequest
{
    [Required]
    [StringLength(150, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(50)]
    public string? Code { get; set; }

    [Required]
    public DiscountType DiscountType { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal Value { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    public decimal? MinimumOrderAmount { get; set; }
    public int? UsageLimit { get; set; }
    public List<Guid> ProductIds { get; set; } = new();
}

public class UpdateDiscountRequest : CreateDiscountRequest
{
    public bool IsActive { get; set; } = true;
}

public class AssignProductToDiscountRequest
{
    [Required]
    public Guid ProductId { get; set; }
}
