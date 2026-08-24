namespace ECommerce.Application.DTOs.Dashboard;

public class AdminDashboardDto
{
    public KpiSummaryDto Kpis { get; set; } = new();
    public List<SalesTrendPointDto> SalesTrend30Days { get; set; } = new();
    public List<SalesTrendPointDto> SalesTrend12Months { get; set; } = new();
    public List<OrderStatusCountDto> OrderStatusDistribution { get; set; } = new();
    public List<TopSellingProductDto> TopSellingProducts { get; set; } = new();
    public List<RecentDashboardOrderDto> RecentOrders { get; set; } = new();
    public List<LowStockProductDto> LowStockProducts { get; set; } = new();
    public int TotalProductsCount { get; set; }
    public int TotalCustomersCount { get; set; }
    public int TotalOrdersCount { get; set; }
}

public class KpiSummaryDto
{
    public decimal TotalRevenueThisMonth { get; set; }
    public decimal RevenueChangePercentage { get; set; }

    public int TotalOrdersThisMonth { get; set; }
    public decimal OrdersChangePercentage { get; set; }

    public int NewCustomersThisMonth { get; set; }
    public decimal CustomersChangePercentage { get; set; }

    public decimal AverageOrderValue { get; set; }
    public decimal AovChangePercentage { get; set; }
}

public class SalesTrendPointDto
{
    public string DateKey { get; set; } = string.Empty;
    public string DateLabel { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
}

public class OrderStatusCountDto
{
    public OrderStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}

public class TopSellingProductDto
{
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int UnitsSold { get; set; }
    public decimal TotalRevenue { get; set; }
}

public class RecentDashboardOrderDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public decimal TotalAmount { get; set; }
    public int ItemCount { get; set; }
}

public class LowStockProductDto
{
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
    public decimal Price { get; set; }
}
