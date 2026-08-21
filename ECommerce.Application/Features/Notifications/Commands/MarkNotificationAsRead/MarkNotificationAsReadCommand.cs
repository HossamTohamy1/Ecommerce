namespace ECommerce.Application.Features.Notifications.Commands.MarkNotificationAsRead;

public record MarkNotificationAsReadCommand(string UserId, Guid Id) : IRequest<Result>;
