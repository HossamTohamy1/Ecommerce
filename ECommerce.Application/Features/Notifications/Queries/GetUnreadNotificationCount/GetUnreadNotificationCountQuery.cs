namespace ECommerce.Application.Features.Notifications.Queries.GetUnreadNotificationCount;

public record GetUnreadNotificationCountQuery(string UserId) : IRequest<int>;
