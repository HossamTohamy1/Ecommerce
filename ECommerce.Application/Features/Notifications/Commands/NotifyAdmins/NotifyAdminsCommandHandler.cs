using ECommerce.Application.DTOs.Notifications;
using ECommerce.Shared.Constants;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Application.Features.Notifications.Commands.NotifyAdmins;

public class NotifyAdminsCommandHandler : IRequestHandler<NotifyAdminsCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IRealtimeNotifier _realtimeNotifier;

    public NotifyAdminsCommandHandler(IApplicationDbContext context, IRealtimeNotifier realtimeNotifier)
    {
        _context = context;
        _realtimeNotifier = realtimeNotifier;
    }

    public async Task Handle(NotifyAdminsCommand command, CancellationToken ct)
    {
        var adminIds = (await _context.Set<IdentityUserRole<Guid>>()
                .Join(_context.Set<IdentityRole<Guid>>().Where(r => r.Name == AppConstants.Roles.Admin),
                    ur => ur.RoleId, r => r.Id, (ur, r) => ur.UserId)
                .ToListAsync(ct))
            .Select(id => id.ToString())
            .ToList();

        if (adminIds.Count == 0)
        {
            return;
        }

        var now = DateTime.UtcNow;
        var notifications = adminIds.Select(adminId => new Notification
        {
            UserId = adminId,
            Type = command.Type,
            Title = command.Title,
            Message = command.Message,
            Url = command.Url,
            CreatedById = adminId,
            CreatedAt = now
        }).ToList();

        _context.Set<Notification>().AddRange(notifications);
        await _context.SaveChangesAsync(ct);

        var payload = new NotificationPushPayload(
            notifications[0].Id,
            notifications[0].Type.ToString(),
            notifications[0].Title,
            notifications[0].Message,
            notifications[0].Url,
            notifications[0].CreatedAt);

        await _realtimeNotifier.PushNotificationToAdminsAsync(payload, ct);
    }
}
