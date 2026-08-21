namespace ECommerce.Application.Features.Notifications.Commands.MarkNotificationAsRead;

public class MarkNotificationAsReadCommandHandler : IRequestHandler<MarkNotificationAsReadCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public MarkNotificationAsReadCommandHandler(IApplicationDbContext context, IStringLocalizer<SharedResource> localizer)
    {
        _context = context;
        _localizer = localizer;
    }

    public async Task<Result> Handle(MarkNotificationAsReadCommand command, CancellationToken ct)
    {
        var notification = await _context.Set<Notification>().FirstOrDefaultAsync(n => n.Id == command.Id && n.UserId == command.UserId, ct);
        if (notification is null)
        {
            return Result.Failure(_localizer["Notification.NotFound"].Value);
        }

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
        }

        return Result.Success();
    }
}
