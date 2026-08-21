namespace ECommerce.Application.Features.Notifications.Commands.NotifyAdmins;

public record NotifyAdminsCommand(
    NotificationType Type,
    string Title,
    string Message,
    string? Url = null) : IRequest;
