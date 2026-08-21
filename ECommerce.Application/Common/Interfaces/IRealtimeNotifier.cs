namespace ECommerce.Application.Common.Interfaces;

public record NotificationPushPayload(
    Guid Id,
    string Type,
    string Title,
    string Message,
    string? Url,
    DateTime CreatedAt);

public record ChatMessagePushPayload(
    Guid Id,
    Guid ConversationId,
    string SenderId,
    string SenderName,
    string SenderRole,
    string Content,
    DateTime CreatedAt);

public interface IRealtimeNotifier
{
    Task PushNotificationToUserAsync(string userId, NotificationPushPayload payload, CancellationToken ct = default);

    Task PushNotificationToAdminsAsync(NotificationPushPayload payload, CancellationToken ct = default);

    Task PushChatMessageAsync(Guid conversationId, ChatMessagePushPayload payload, CancellationToken ct = default);
}
