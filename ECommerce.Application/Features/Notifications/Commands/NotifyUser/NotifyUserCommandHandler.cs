using ECommerce.Application.DTOs.Notifications;

namespace ECommerce.Application.Features.Notifications.Commands.NotifyUser;

public class NotifyUserCommandHandler : IRequestHandler<NotifyUserCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IRealtimeNotifier _realtimeNotifier;

    public NotifyUserCommandHandler(IApplicationDbContext context, IRealtimeNotifier realtimeNotifier)
    {
        _context = context;
        _realtimeNotifier = realtimeNotifier;
    }

    public async Task Handle(NotifyUserCommand command, CancellationToken ct)
    {
        var notification = new Notification
        {
            UserId = command.UserId,
            Type = command.Type,
            Title = command.Title,
            Message = command.Message,
            Url = command.Url,
            CreatedById = command.UserId
        };

        _context.Set<Notification>().Add(notification);
        await _context.SaveChangesAsync(ct);

        var payload = new NotificationPushPayload(
            notification.Id,
            notification.Type.ToString(),
            notification.Title,
            notification.Message,
            notification.Url,
            notification.CreatedAt);

        await _realtimeNotifier.PushNotificationToUserAsync(command.UserId, payload, ct);
    }
}
