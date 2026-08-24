using ECommerce.Application.DTOs.Dashboard;
using ECommerce.Application.Features.Dashboard.Queries.GetAdminDashboard;

namespace ECommerce.API.Pages;

public class IndexModel : PageModel
{
    private readonly IMediator _mediator;

    public IndexModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    public bool IsAdmin => User.IsInRole(AppConstants.Roles.Admin);

    public AdminDashboardDto? DashboardData { get; set; }

    public async Task OnGetAsync()
    {
        if (IsAdmin)
        {
            DashboardData = await _mediator.Send(new GetAdminDashboardQuery(10));
        }
    }
}
