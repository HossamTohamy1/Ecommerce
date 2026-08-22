using ECommerce.Application.DTOs.Catalog;
using Microsoft.Extensions.Caching.Memory;

namespace ECommerce.Application.Features.Brands.Queries.GetAllBrands;

public class CachedGetAllBrandsQueryHandler : IRequestHandler<GetAllBrandsQuery, List<BrandDto>>
{
    private readonly GetAllBrandsQueryHandler _inner;
    private readonly IMemoryCache _cache;
    private const string CacheKey = "catalog:brands";

    public CachedGetAllBrandsQueryHandler(GetAllBrandsQueryHandler inner, IMemoryCache cache)
    {
        _inner = inner;
        _cache = cache;
    }

    public async Task<List<BrandDto>> Handle(GetAllBrandsQuery request, CancellationToken ct)
    {
        return (await _cache.GetOrCreateAsync(CacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
            return await _inner.Handle(request, ct);
        })) ?? new List<BrandDto>();
    }
}
