using System.Diagnostics;
using ECommerce.Application.DTOs.Dashboard;
using Microsoft.Extensions.Caching.Memory;

namespace ECommerce.Application.Features.Dashboard.Queries.GetAdminDashboard;

public class CachedGetAdminDashboardQueryHandler : IRequestHandler<GetAdminDashboardQuery, AdminDashboardDto>
{
    private readonly GetAdminDashboardQueryHandler _inner;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CachedGetAdminDashboardQueryHandler> _logger;
    private const string CacheKeyPrefix = "dashboard:admin:summary";

    public CachedGetAdminDashboardQueryHandler(
        GetAdminDashboardQueryHandler inner,
        IMemoryCache cache,
        ILogger<CachedGetAdminDashboardQueryHandler> logger)
    {
        _inner = inner;
        _cache = cache;
        _logger = logger;
    }

    public async Task<AdminDashboardDto> Handle(GetAdminDashboardQuery request, CancellationToken ct)
    {
        var cacheKey = $"{CacheKeyPrefix}:{request.LowStockThreshold}";
        var sw = Stopwatch.StartNew();
        var isMiss = false;

        var result = await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            isMiss = true;
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(3);
            return await _inner.Handle(request, ct);
        }) ?? new AdminDashboardDto();

        sw.Stop();

        if (isMiss)
        {
            _logger.LogInformation("[CachedGetAdminDashboardQueryHandler] CACHE MISS - computed fresh dashboard metrics (took {ElapsedMilliseconds} ms)", sw.ElapsedMilliseconds);
        }
        else
        {
            _logger.LogInformation("[CachedGetAdminDashboardQueryHandler] CACHE HIT (took {ElapsedMilliseconds} ms)", sw.ElapsedMilliseconds);
        }

        return result;
    }
}
