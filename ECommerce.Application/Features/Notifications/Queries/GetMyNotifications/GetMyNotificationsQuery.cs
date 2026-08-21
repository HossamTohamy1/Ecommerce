using ECommerce.Application.DTOs.Notifications;

namespace ECommerce.Application.Features.Notifications.Queries.GetMyNotifications;

public record GetMyNotificationsQuery(string UserId, int Page, int PageSize) : IRequest<PagedResult<NotificationDto>>;
