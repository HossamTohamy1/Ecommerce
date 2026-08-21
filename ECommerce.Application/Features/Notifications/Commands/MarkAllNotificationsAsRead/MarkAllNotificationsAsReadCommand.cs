namespace ECommerce.Application.Features.Notifications.Commands.MarkAllNotificationsAsRead;

public record MarkAllNotificationsAsReadCommand(string UserId) : IRequest<Result>;
