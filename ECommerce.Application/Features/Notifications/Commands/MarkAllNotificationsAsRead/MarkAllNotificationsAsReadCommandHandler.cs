namespace ECommerce.Application.Features.Notifications.Commands.MarkAllNotificationsAsRead;

public class MarkAllNotificationsAsReadCommandHandler : IRequestHandler<MarkAllNotificationsAsReadCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public MarkAllNotificationsAsReadCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(MarkAllNotificationsAsReadCommand command, CancellationToken ct)
    {
        var unread = await _context.Set<Notification>().Where(n => n.UserId == command.UserId && !n.IsRead).ToListAsync(ct);
        var now = DateTime.UtcNow;
        foreach (var notification in unread)
        {
            notification.IsRead = true;
            notification.ReadAt = now;
        }

        if (unread.Count > 0)
        {
            await _context.SaveChangesAsync(ct);
        }

        return Result.Success();
    }
}
