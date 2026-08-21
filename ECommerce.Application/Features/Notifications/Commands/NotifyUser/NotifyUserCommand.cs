namespace ECommerce.Application.Features.Notifications.Commands.NotifyUser;

public record NotifyUserCommand(
    string UserId,
    NotificationType Type,
    string Title,
    string Message,
    string? Url = null) : IRequest;
