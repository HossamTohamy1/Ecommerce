using System.Diagnostics;
using ECommerce.Application.DTOs.Catalog;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace ECommerce.Application.Features.Categories.Queries.GetAllCategories;

public class CachedGetAllCategoriesQueryHandler : IRequestHandler<GetAllCategoriesQuery, List<CategoryDto>>
{
    private readonly GetAllCategoriesQueryHandler _inner;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CachedGetAllCategoriesQueryHandler> _logger;
    private const string CacheKey = "catalog:categories";

    public CachedGetAllCategoriesQueryHandler(
        GetAllCategoriesQueryHandler inner,
        IMemoryCache cache,
        ILogger<CachedGetAllCategoriesQueryHandler> logger)
    {
        _inner = inner;
        _cache = cache;
        _logger = logger;
    }

    public async Task<List<CategoryDto>> Handle(GetAllCategoriesQuery request, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        var isMiss = false;

        var result = (await _cache.GetOrCreateAsync(CacheKey, async entry =>
        {
            isMiss = true;
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
            return await _inner.Handle(request, ct);
        })) ?? new List<CategoryDto>();

        sw.Stop();

        if (isMiss)
        {
            _logger.LogInformation("[CachedGetAllCategoriesQueryHandler] CACHE MISS - hit database (took {ElapsedMilliseconds} ms)", sw.ElapsedMilliseconds);
        }
        else
        {
            _logger.LogInformation("[CachedGetAllCategoriesQueryHandler] CACHE HIT (took {ElapsedMilliseconds} ms)", sw.ElapsedMilliseconds);
        }

        return result;
    }
}
