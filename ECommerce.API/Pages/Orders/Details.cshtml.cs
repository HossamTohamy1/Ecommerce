using ECommerce.Application.DTOs.Orders;
using ECommerce.Application.Features.Orders.Commands.UpdateOrderStatus;
using ECommerce.Application.Features.Orders.Queries.GetOrderById;
using ECommerce.Domain.Entities;

namespace ECommerce.API.Pages.Orders;

public class DetailsModel : RazorPageBase
{
    private readonly IMediator _mediator;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public DetailsModel(IMediator mediator, IStringLocalizer<SharedResource> localizer)
    {
        _mediator = mediator;
        _localizer = localizer;
    }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public OrderDto? Order { get; set; }

    [BindProperty]
    public OrderStatus NewStatus { get; set; }

    [BindProperty]
    public string? StatusNote { get; set; }

    public async Task<IActionResult> OnGetAsync([FromQuery] Guid? id)
    {
        if (id.HasValue && id.Value != Guid.Empty)
        {
            Id = id.Value;
        }

        if (Id == Guid.Empty)
        {
            SetError(_localizer["Order.NotFound"].Value);
            return RedirectToPage(User.IsInRole(AppConstants.Roles.Admin) ? "/Orders/Admin/AllOrders" : "/Orders/Index");
        }

        var isAdmin = User.IsInRole(AppConstants.Roles.Admin);
        var result = await _mediator.Send(new GetOrderByIdQuery(Id, CurrentUserId, isAdmin));
        if (!result.Succeeded || result.Data is null)
        {
            SetError(result.Error ?? _localizer["Order.NotFound"].Value);
            return RedirectToPage(User.IsInRole(AppConstants.Roles.Admin) ? "/Orders/Admin/AllOrders" : "/Orders/Index");
        }

        Order = result.Data;
        return Page();
    }

    public async Task<IActionResult> OnPostUpdateStatusAsync()
    {
        if (!User.IsInRole(AppConstants.Roles.Admin))
        {
            return Forbid();
        }

        var result = await _mediator.Send(new UpdateOrderStatusCommand(Id, new UpdateOrderStatusRequest
        {
            Status = NewStatus,
            Note = StatusNote
        }, CurrentUserId));

        if (result.Succeeded) SetSuccess(_localizer); else SetError(result.Error);
        return RedirectToPage(new { id = Id });
    }
}

