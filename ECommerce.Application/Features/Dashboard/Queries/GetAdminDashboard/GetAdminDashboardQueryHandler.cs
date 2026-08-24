using System.Globalization;
using ECommerce.Application.DTOs.Dashboard;

namespace ECommerce.Application.Features.Dashboard.Queries.GetAdminDashboard;

public class GetAdminDashboardQueryHandler : IRequestHandler<GetAdminDashboardQuery, AdminDashboardDto>
{
    private readonly IApplicationDbContext _context;

    public GetAdminDashboardQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AdminDashboardDto> Handle(GetAdminDashboardQuery request, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var currentMonthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var nextMonthStart = currentMonthStart.AddMonths(1);
        var prevMonthStart = currentMonthStart.AddMonths(-1);
        var prevMonthEnd = currentMonthStart;

        // 1. KPI Queries
        var currentMonthOrders = await _context.Set<Order>()
            .AsNoTracking()
            .Where(o => o.CreatedAt >= currentMonthStart && o.CreatedAt < nextMonthStart && o.Status != OrderStatus.Cancelled)
            .Select(o => new { Total = o.TotalAmount.Amount })
            .ToListAsync(ct);

        var prevMonthOrders = await _context.Set<Order>()
            .AsNoTracking()
            .Where(o => o.CreatedAt >= prevMonthStart && o.CreatedAt < prevMonthEnd && o.Status != OrderStatus.Cancelled)
            .Select(o => new { Total = o.TotalAmount.Amount })
            .ToListAsync(ct);

        var currentRevenue = currentMonthOrders.Sum(o => o.Total);
        var prevRevenue = prevMonthOrders.Sum(o => o.Total);
        var revenueChange = CalculatePercentageChange(prevRevenue, currentRevenue);

        var currentOrdersCount = currentMonthOrders.Count;
        var prevOrdersCount = prevMonthOrders.Count;
        var ordersChange = CalculatePercentageChange(prevOrdersCount, currentOrdersCount);

        var currentNewCustomers = await _context.Set<ApplicationUser>()
            .AsNoTracking()
            .CountAsync(u => u.CreatedAtUtc >= currentMonthStart && u.CreatedAtUtc < nextMonthStart, ct);

        var prevNewCustomers = await _context.Set<ApplicationUser>()
            .AsNoTracking()
            .CountAsync(u => u.CreatedAtUtc >= prevMonthStart && u.CreatedAtUtc < prevMonthEnd, ct);

        var customersChange = CalculatePercentageChange(prevNewCustomers, currentNewCustomers);

        var currentAov = currentOrdersCount > 0 ? currentRevenue / currentOrdersCount : 0m;
        var prevAov = prevOrdersCount > 0 ? prevRevenue / prevOrdersCount : 0m;
        var aovChange = CalculatePercentageChange(prevAov, currentAov);

        var kpis = new KpiSummaryDto
        {
            TotalRevenueThisMonth = Math.Round(currentRevenue, 2),
            RevenueChangePercentage = Math.Round(revenueChange, 1),
            TotalOrdersThisMonth = currentOrdersCount,
            OrdersChangePercentage = Math.Round(ordersChange, 1),
            NewCustomersThisMonth = currentNewCustomers,
            CustomersChangePercentage = Math.Round(customersChange, 1),
            AverageOrderValue = Math.Round(currentAov, 2),
            AovChangePercentage = Math.Round(aovChange, 1)
        };

        // 2. Sales Trend: Last 30 Days
        var thirtyDaysAgo = now.Date.AddDays(-29);
        var recentOrdersForTrend = await _context.Set<Order>()
            .AsNoTracking()
            .Where(o => o.CreatedAt >= thirtyDaysAgo && o.Status != OrderStatus.Cancelled)
            .Select(o => new { o.CreatedAt, Total = o.TotalAmount.Amount })
            .ToListAsync(ct);

        var dailyTrend = new List<SalesTrendPointDto>();
        for (var i = 0; i < 30; i++)
        {
            var day = thirtyDaysAgo.AddDays(i);
            var dayOrders = recentOrdersForTrend.Where(o => o.CreatedAt.Date == day.Date).ToList();
            dailyTrend.Add(new SalesTrendPointDto
            {
                DateKey = day.ToString("yyyy-MM-dd"),
                DateLabel = day.ToString("MMM dd", CultureInfo.InvariantCulture),
                Revenue = Math.Round(dayOrders.Sum(o => o.Total), 2),
                OrderCount = dayOrders.Count
            });
        }

        // 3. Sales Trend: Last 12 Months
        var twelveMonthsAgo = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-11);
        var annualOrdersForTrend = await _context.Set<Order>()
            .AsNoTracking()
            .Where(o => o.CreatedAt >= twelveMonthsAgo && o.Status != OrderStatus.Cancelled)
            .Select(o => new { o.CreatedAt, Total = o.TotalAmount.Amount })
            .ToListAsync(ct);

        var monthlyTrend = new List<SalesTrendPointDto>();
        for (var i = 0; i < 12; i++)
        {
            var month = twelveMonthsAgo.AddMonths(i);
            var monthOrders = annualOrdersForTrend
                .Where(o => o.CreatedAt.Year == month.Year && o.CreatedAt.Month == month.Month)
                .ToList();

            monthlyTrend.Add(new SalesTrendPointDto
            {
                DateKey = month.ToString("yyyy-MM"),
                DateLabel = month.ToString("MMM yyyy", CultureInfo.InvariantCulture),
                Revenue = Math.Round(monthOrders.Sum(o => o.Total), 2),
                OrderCount = monthOrders.Count
            });
        }

        // 4. Order Status Distribution
        var statusGroups = await _context.Set<Order>()
            .AsNoTracking()
            .GroupBy(o => o.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var totalOrdersAllTime = statusGroups.Sum(g => g.Count);
        var statusDistribution = Enum.GetValues<OrderStatus>()
            .Select(status =>
            {
                var count = statusGroups.FirstOrDefault(g => g.Status == status)?.Count ?? 0;
                var percentage = totalOrdersAllTime > 0 ? (decimal)count * 100m / totalOrdersAllTime : 0m;
                return new OrderStatusCountDto
                {
                    Status = status,
                    StatusName = status.ToString(),
                    Count = count,
                    Percentage = Math.Round(percentage, 1)
                };
            })
            .ToList();

        // 5. Top Selling Products
        var nonCancelledOrderItems = await _context.Set<OrderItem>()
            .AsNoTracking()
            .Where(oi => oi.Order.Status != OrderStatus.Cancelled)
            .Select(oi => new
            {
                oi.ProductId,
                oi.Quantity,
                UnitPrice = oi.UnitPrice.Amount,
                DiscountApplied = oi.DiscountApplied.Amount
            })
            .ToListAsync(ct);

        var topItems = nonCancelledOrderItems
            .GroupBy(oi => oi.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                UnitsSold = g.Sum(x => x.Quantity),
                TotalRevenue = g.Sum(x => x.UnitPrice * x.Quantity - x.DiscountApplied)
            })
            .OrderByDescending(x => x.TotalRevenue)
            .Take(5)
            .ToList();

        var topProductIds = topItems.Select(t => t.ProductId).ToList();
        var productsInfo = await _context.Set<Product>()
            .AsNoTracking()
            .Where(p => topProductIds.Contains(p.Id))
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.SKU,
                CategoryName = p.Category.Name,
                ImageUrl = p.Images.Where(i => i.IsMain).Select(i => i.ImageUrl).FirstOrDefault()
                    ?? p.Images.Select(i => i.ImageUrl).FirstOrDefault()
            })
            .ToDictionaryAsync(p => p.Id, ct);

        var topProducts = topItems.Select(t =>
        {
            var info = productsInfo.GetValueOrDefault(t.ProductId);
            return new TopSellingProductDto
            {
                ProductId = t.ProductId,
                Name = info?.Name ?? "Unknown Product",
                SKU = info?.SKU ?? string.Empty,
                ImageUrl = info?.ImageUrl,
                CategoryName = info?.CategoryName ?? string.Empty,
                UnitsSold = t.UnitsSold,
                TotalRevenue = Math.Round(t.TotalRevenue, 2)
            };
        }).ToList();

        // 6. Recent Orders
        var recentOrdersRaw = await _context.Set<Order>()
            .AsNoTracking()
            .OrderByDescending(o => o.CreatedAt)
            .Take(8)
            .Select(o => new
            {
                o.Id,
                OrderNumber = o.OrderNumber.Value,
                o.UserId,
                o.Status,
                o.CreatedAt,
                TotalAmount = o.TotalAmount.Amount,
                ItemCount = o.Items.Count,
                RecipientName = o.ShippingAddress.FullName
            })
            .ToListAsync(ct);

        var recentUserGuids = recentOrdersRaw
            .Select(r => r.UserId)
            .Distinct()
            .Where(id => Guid.TryParse(id, out _))
            .Select(Guid.Parse)
            .ToList();

        var customerNames = await _context.Set<ApplicationUser>()
            .AsNoTracking()
            .Where(u => recentUserGuids.Contains(u.Id))
            .Select(u => new { u.Id, u.FullName, u.Email })
            .ToDictionaryAsync(u => u.Id.ToString(), u => new { u.FullName, u.Email }, ct);

        var recentOrders = recentOrdersRaw.Select(r =>
        {
            var customer = customerNames.GetValueOrDefault(r.UserId);
            var customerName = !string.IsNullOrWhiteSpace(customer?.FullName)
                ? customer.FullName
                : (!string.IsNullOrWhiteSpace(r.RecipientName) ? r.RecipientName : "Guest / Unknown");

            return new RecentDashboardOrderDto
            {
                Id = r.Id,
                OrderNumber = r.OrderNumber,
                CustomerName = customerName,
                CustomerEmail = customer?.Email,
                Status = r.Status,
                CreatedAt = r.CreatedAt,
                TotalAmount = Math.Round(r.TotalAmount, 2),
                ItemCount = r.ItemCount
            };
        }).ToList();

        // 7. Low Stock Products
        var threshold = request.LowStockThreshold <= 0 ? 10 : request.LowStockThreshold;
        var lowStock = await _context.Set<Product>()
            .AsNoTracking()
            .Where(p => p.IsActive && !p.IsDeleted && p.StockQuantity <= threshold)
            .OrderBy(p => p.StockQuantity)
            .Take(10)
            .Select(p => new LowStockProductDto
            {
                ProductId = p.Id,
                Name = p.Name,
                SKU = p.SKU,
                ImageUrl = p.Images.Where(i => i.IsMain).Select(i => i.ImageUrl).FirstOrDefault()
                    ?? p.Images.Select(i => i.ImageUrl).FirstOrDefault(),
                CategoryName = p.Category.Name,
                StockQuantity = p.StockQuantity,
                Price = p.Price
            })
            .ToListAsync(ct);

        // 8. Overall Total Counts
        var totalProducts = await _context.Set<Product>().CountAsync(p => !p.IsDeleted, ct);
        var totalCustomers = await _context.Set<ApplicationUser>().CountAsync(ct);

        return new AdminDashboardDto
        {
            Kpis = kpis,
            SalesTrend30Days = dailyTrend,
            SalesTrend12Months = monthlyTrend,
            OrderStatusDistribution = statusDistribution,
            TopSellingProducts = topProducts,
            RecentOrders = recentOrders,
            LowStockProducts = lowStock,
            TotalProductsCount = totalProducts,
            TotalCustomersCount = totalCustomers,
            TotalOrdersCount = totalOrdersAllTime
        };
    }

    private static decimal CalculatePercentageChange(decimal previous, decimal current)
    {
        if (previous == 0)
        {
            return current > 0 ? 100m : 0m;
        }

        return ((current - previous) / previous) * 100m;
    }
}
