namespace ECommerce.Application.Features.Discounts.Common;

public class DiscountResolver : IDiscountResolver
{
    private readonly IApplicationDbContext _context;

    public DiscountResolver(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Discount?> GetActiveDiscountForProductAsync(Guid productId, DateTime now, CancellationToken ct = default)
    {
        return await _context.Set<ProductDiscount>()
            .Where(pd => pd.ProductId == productId)
            .Select(pd => pd.Discount)
            .Where(d => d.IsActive
                        && d.StartDate <= now
                        && d.EndDate >= now
                        && (d.UsageLimit == null || d.UsageCount < d.UsageLimit))
            .OrderByDescending(d => d.Value)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<Dictionary<Guid, Discount>> GetActiveDiscountsForProductsAsync(IEnumerable<Guid> productIds, DateTime now, CancellationToken ct = default)
    {
        var idList = productIds.Distinct().ToList();
        if (idList.Count == 0)
        {
            return new Dictionary<Guid, Discount>();
        }

        var list = await _context.Set<ProductDiscount>()
            .Where(pd => idList.Contains(pd.ProductId))
            .Select(pd => new { pd.ProductId, pd.Discount })
            .Where(x => x.Discount.IsActive
                        && x.Discount.StartDate <= now
                        && x.Discount.EndDate >= now
                        && (x.Discount.UsageLimit == null || x.Discount.UsageCount < x.Discount.UsageLimit))
            .ToListAsync(ct);

        return list
            .GroupBy(x => x.ProductId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(x => x.Discount.Value).First().Discount);
    }

    public async Task<Dictionary<Guid, Discount>> GetAllActiveDiscountsAsync(DateTime now, CancellationToken ct = default)
    {
        return await _context.Set<ProductDiscount>()
            .Where(x => x.Discount.IsActive
                        && x.Discount.StartDate <= now
                        && x.Discount.EndDate >= now
                        && (x.Discount.UsageLimit == null || x.Discount.UsageCount < x.Discount.UsageLimit))
            .GroupBy(x => x.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                Discount = g.OrderByDescending(x => x.Discount.Value).First().Discount
            })
            .ToDictionaryAsync(x => x.ProductId, x => x.Discount, ct);
    }

    public decimal CalculateDiscountAmount(decimal price, Discount discount)
    {
        if (discount.DiscountType == DiscountType.Percentage)
        {
            return Math.Round(price * discount.Value / 100m, 2);
        }

        return Math.Min(discount.Value, price);
    }

    public decimal CalculateDiscountedPrice(decimal price, Discount discount)
    {
        var amount = CalculateDiscountAmount(price, discount);
        return Math.Max(0, price - amount);
    }
}
