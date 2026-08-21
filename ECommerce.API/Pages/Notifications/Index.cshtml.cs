using ECommerce.Application.DTOs.Notifications;
using ECommerce.Application.Features.Notifications.Commands.MarkAllNotificationsAsRead;
using ECommerce.Application.Features.Notifications.Queries.GetMyNotifications;

namespace ECommerce.API.Pages.Notifications;

public class IndexModel : RazorPageBase
{
    private readonly IMediator _mediator;

    public IndexModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    [BindProperty(SupportsGet = true, Name = "page")]
    public int PageNumber { get; set; } = 1;

    public PagedResult<NotificationDto> Result { get; set; } = new();

    public async Task OnGetAsync([FromQuery] int? page, [FromQuery] int? pageNumber)
    {
        var p = (page.HasValue && page.Value > 0) ? page.Value :
                ((pageNumber.HasValue && pageNumber.Value > 0) ? pageNumber.Value :
                (PageNumber > 0 ? PageNumber : 1));

        PageNumber = p;
        Result = await _mediator.Send(new GetMyNotificationsQuery(CurrentUserId, p, 20));
    }

    public async Task<IActionResult> OnPostMarkAllReadAsync()
    {
        await _mediator.Send(new MarkAllNotificationsAsReadCommand(CurrentUserId));
        return RedirectToPage();
    }
}

