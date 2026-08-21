namespace ECommerce.Application.Features.Notifications.Queries.GetUnreadNotificationCount;

public class GetUnreadNotificationCountQueryHandler : IRequestHandler<GetUnreadNotificationCountQuery, int>
{
    private readonly IApplicationDbContext _context;

    public GetUnreadNotificationCountQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public Task<int> Handle(GetUnreadNotificationCountQuery request, CancellationToken ct)
    {
        return _context.Set<Notification>().CountAsync(n => n.UserId == request.UserId && !n.IsRead, ct);
    }
}
