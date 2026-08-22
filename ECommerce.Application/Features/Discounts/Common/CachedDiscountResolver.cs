using System.Diagnostics;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace ECommerce.Application.Features.Discounts.Common;

public class CachedDiscountResolver : IDiscountResolver
{
    private readonly IDiscountResolver _inner;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CachedDiscountResolver> _logger;
    private const string CacheKey = "discounts:active:all";

    public CachedDiscountResolver(
        IDiscountResolver inner,
        IMemoryCache cache,
        ILogger<CachedDiscountResolver> logger)
    {
        _inner = inner;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Discount?> GetActiveDiscountForProductAsync(Guid productId, DateTime now, CancellationToken ct = default)
    {
        var discounts = await GetActiveDiscountsForProductsAsync(new[] { productId }, now, ct);
        return discounts.TryGetValue(productId, out var discount) ? discount : null;
    }

    public async Task<Dictionary<Guid, Discount>> GetAllActiveDiscountsAsync(DateTime now, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        var isMiss = false;

        var allActive = await _cache.GetOrCreateAsync(CacheKey, async entry =>
        {
            isMiss = true;
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            return await _inner.GetAllActiveDiscountsAsync(now, ct);
        });

        sw.Stop();

        if (isMiss)
        {
            _logger.LogInformation("[CachedDiscountResolver] CACHE MISS - hit database (took {ElapsedMilliseconds} ms)", sw.ElapsedMilliseconds);
        }
        else
        {
            _logger.LogInformation("[CachedDiscountResolver] CACHE HIT (took {ElapsedMilliseconds} ms)", sw.ElapsedMilliseconds);
        }

        return allActive ?? new Dictionary<Guid, Discount>();
    }

    public async Task<Dictionary<Guid, Discount>> GetActiveDiscountsForProductsAsync(IEnumerable<Guid> productIds, DateTime now, CancellationToken ct = default)
    {
        var idList = productIds.Distinct().ToList();
        if (idList.Count == 0)
        {
            return new Dictionary<Guid, Discount>();
        }

        var allActive = await GetAllActiveDiscountsAsync(now, ct);

        return allActive
            .Where(kvp => idList.Contains(kvp.Key))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    public decimal CalculateDiscountAmount(decimal price, Discount discount)
    {
        return _inner.CalculateDiscountAmount(price, discount);
    }

    public decimal CalculateDiscountedPrice(decimal price, Discount discount)
    {
        return _inner.CalculateDiscountedPrice(price, discount);
    }
}
