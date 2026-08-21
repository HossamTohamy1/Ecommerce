using Microsoft.AspNetCore.SignalR;

namespace ECommerce.Infrastructure.Realtime;

public class RealtimeNotifier : IRealtimeNotifier
{
    private readonly IHubContext<NotificationHub> _notificationHub;
    private readonly IHubContext<ChatHub> _chatHub;

    public RealtimeNotifier(IHubContext<NotificationHub> notificationHub, IHubContext<ChatHub> chatHub)
    {
        _notificationHub = notificationHub;
        _chatHub = chatHub;
    }

    public Task PushNotificationToUserAsync(string userId, NotificationPushPayload payload, CancellationToken ct = default)
        => _notificationHub.Clients.Group(NotificationHub.UserGroup(userId)).SendAsync("notificationReceived", payload, ct);

    public Task PushNotificationToAdminsAsync(NotificationPushPayload payload, CancellationToken ct = default)
        => _notificationHub.Clients.Group(NotificationHub.AdminsGroup).SendAsync("notificationReceived", payload, ct);

    public async Task PushChatMessageAsync(Guid conversationId, ChatMessagePushPayload payload, CancellationToken ct = default)
    {
        await _chatHub.Clients.Group(ChatHub.ConversationGroup(conversationId)).SendAsync("messageReceived", payload, ct);
        await _chatHub.Clients.Group(ChatHub.AdminsGroup).SendAsync("messageReceived", payload, ct);
    }
}
