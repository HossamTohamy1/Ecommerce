using ECommerce.Application.DTOs.Notifications;
using ECommerce.Application.Features.Notifications.Commands.MarkAllNotificationsAsRead;
using ECommerce.Application.Features.Notifications.Commands.MarkNotificationAsRead;
using ECommerce.Application.Features.Notifications.Queries.GetMyNotifications;
using ECommerce.Application.Features.Notifications.Queries.GetUnreadNotificationCount;

namespace ECommerce.API.Controllers;

[Route("api/notifications")]
[Authorize]
public class NotificationsController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetMine([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
        => Ok(await _mediator.Send(new GetMyNotificationsQuery(CurrentUserId, page, pageSize), ct));

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount(CancellationToken ct)
        => Ok(new UnreadCountDto { Count = await _mediator.Send(new GetUnreadNotificationCountQuery(CurrentUserId), ct) });

    [HttpPut("{id:guid}/read")]
    [HttpPost("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new MarkNotificationAsReadCommand(CurrentUserId, id), ct);
        return result.Succeeded ? Ok() : NotFound(new { message = result.Error });
    }

    [HttpPut("read-all")]
    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken ct)
    {
        await _mediator.Send(new MarkAllNotificationsAsReadCommand(CurrentUserId), ct);
        return Ok();
    }
}

