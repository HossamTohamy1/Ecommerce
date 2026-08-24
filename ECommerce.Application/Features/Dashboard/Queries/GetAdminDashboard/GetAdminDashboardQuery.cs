using ECommerce.Application.DTOs.Dashboard;

namespace ECommerce.Application.Features.Dashboard.Queries.GetAdminDashboard;

public record GetAdminDashboardQuery(int LowStockThreshold = 10) : IRequest<AdminDashboardDto>;
