namespace ECommerce.Domain.Entities;

public class Discount : BaseEntity
{
    private readonly List<ProductDiscount> _productDiscounts = new();

    public string Name { get; private set; } = string.Empty;
    public string? Code { get; private set; }

    public DiscountType DiscountType { get; private set; }
    public decimal Value { get; private set; }

    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }

    public decimal? MinimumOrderAmount { get; private set; }
    public int? UsageLimit { get; private set; }
    public int UsageCount { get; private set; }

    public IReadOnlyCollection<ProductDiscount> ProductDiscounts => _productDiscounts;

    private Discount()
    {
    }

    public static Discount Create(string name, string? code, DiscountType discountType, decimal value, DateTime startDate, DateTime endDate, decimal? minimumOrderAmount, int? usageLimit, string actorId)
    {
        ValidateDetails(name, discountType, value, startDate, endDate);

        return new Discount
        {
            Name = name.Trim(),
            Code = NormalizeCode(code),
            DiscountType = discountType,
            Value = value,
            StartDate = startDate,
            EndDate = endDate,
            MinimumOrderAmount = minimumOrderAmount,
            UsageLimit = usageLimit,
            CreatedById = actorId
        };
    }

    public void UpdateDetails(string name, string? code, DiscountType discountType, decimal value, DateTime startDate, DateTime endDate, decimal? minimumOrderAmount, int? usageLimit, bool isActive, string actorId)
    {
        ValidateDetails(name, discountType, value, startDate, endDate);

        Name = name.Trim();
        Code = NormalizeCode(code);
        DiscountType = discountType;
        Value = value;
        StartDate = startDate;
        EndDate = endDate;
        MinimumOrderAmount = minimumOrderAmount;
        UsageLimit = usageLimit;
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;
        UpdatedById = actorId;
    }

    public bool IsCurrentlyValid(DateTime now) =>
        IsActive && StartDate <= now && EndDate >= now && (UsageLimit is null || UsageCount < UsageLimit);

    public void RecordUsage()
    {
        if (UsageLimit is not null && UsageCount >= UsageLimit)
        {
            throw new DomainException("Discount.UsageLimitReached", "This discount has reached its usage limit.");
        }

        UsageCount++;
    }

    public Money CalculateDiscountAmount(Money lineTotal)
    {
        var amount = DiscountType == DiscountType.Percentage
            ? Math.Round(lineTotal.Amount * Value / 100m, 2)
            : Math.Min(Value, lineTotal.Amount);

        return Money.Of(amount);
    }

    public void AssignProduct(Guid productId, string actorId)
    {
        if (_productDiscounts.Any(pd => pd.ProductId == productId))
        {
            throw new DomainException("Discount.ProductAlreadyAssigned", "This product is already assigned to the discount.");
        }

        _productDiscounts.Add(ProductDiscount.Create(Id, productId, actorId));
    }

    public void RemoveProduct(Guid productId)
    {
        var link = _productDiscounts.FirstOrDefault(pd => pd.ProductId == productId);
        if (link is null)
        {
            throw new DomainException("Discount.ProductNotAssigned", "This product is not assigned to the discount.");
        }

        _productDiscounts.Remove(link);
    }

    private static void ValidateDetails(string name, DiscountType discountType, decimal value, DateTime startDate, DateTime endDate)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Discount.NameRequired", "A discount must have a name.");
        }

        if (endDate <= startDate)
        {
            throw new DomainException("Discount.EndDateAfterStartDate", "The end date must be after the start date.");
        }

        if (discountType == DiscountType.Percentage && (value <= 0 || value > 100))
        {
            throw new DomainException("Discount.PercentageRange", "A percentage discount must be between 0 and 100.");
        }

        if (discountType == DiscountType.FixedAmount && value <= 0)
        {
            throw new DomainException("Discount.ValueMustBePositive", "The discount value must be greater than zero.");
        }
    }

    private static string? NormalizeCode(string? code) => string.IsNullOrWhiteSpace(code) ? null : code.Trim().ToUpperInvariant();
}

public enum DiscountType
{
    Percentage = 1,
    FixedAmount = 2
}
