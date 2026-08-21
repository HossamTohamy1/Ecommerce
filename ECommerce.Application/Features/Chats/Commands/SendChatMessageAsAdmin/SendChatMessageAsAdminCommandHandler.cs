using ECommerce.Application.DTOs.Chat;
using ECommerce.Application.Features.Notifications.Commands.NotifyUser;

namespace ECommerce.Application.Features.Chats.Commands.SendChatMessageAsAdmin;

public class SendChatMessageAsAdminCommandHandler : IRequestHandler<SendChatMessageAsAdminCommand, Result<ChatMessageDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IRealtimeNotifier _realtimeNotifier;
    private readonly IMediator _mediator;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public SendChatMessageAsAdminCommandHandler(
        IApplicationDbContext context,
        IRealtimeNotifier realtimeNotifier,
        IMediator mediator,
        IStringLocalizer<SharedResource> localizer)
    {
        _context = context;
        _realtimeNotifier = realtimeNotifier;
        _mediator = mediator;
        _localizer = localizer;
    }

    public async Task<Result<ChatMessageDto>> Handle(SendChatMessageAsAdminCommand command, CancellationToken ct)
    {
        var conversation = await _context.Set<ChatConversation>().FirstOrDefaultAsync(c => c.Id == command.ConversationId, ct);
        if (conversation is null)
        {
            return Result<ChatMessageDto>.Failure(_localizer["Chat.ConversationNotFound"].Value);
        }

        var message = new ChatMessage
        {
            ConversationId = conversation.Id,
            SenderId = command.AdminId,
            SenderName = command.AdminName,
            SenderRole = ChatSenderRole.Admin,
            Content = command.Request.Content.Trim(),
            CreatedById = command.AdminId
        };

        _context.Set<ChatMessage>().Add(message);

        conversation.LastMessageAt = DateTime.UtcNow;
        conversation.LastMessagePreview = message.Content.Length <= 150 ? message.Content : message.Content[..150] + "...";
        conversation.HasUnreadForCustomer = true;
        conversation.UpdatedAt = DateTime.UtcNow;
        conversation.UpdatedById = command.AdminId;

        await _context.SaveChangesAsync(ct);

        var dto = new ChatMessageDto
        {
            Id = message.Id,
            ConversationId = message.ConversationId,
            SenderId = message.SenderId,
            SenderName = message.SenderName,
            SenderRole = message.SenderRole,
            Content = message.Content,
            IsRead = message.IsRead,
            CreatedAt = message.CreatedAt
        };

        var pushPayload = new ChatMessagePushPayload(
            dto.Id, dto.ConversationId, dto.SenderId, dto.SenderName, dto.SenderRole.ToString(), dto.Content, dto.CreatedAt);

        await _realtimeNotifier.PushChatMessageAsync(conversation.Id, pushPayload, ct);
        await _mediator.Send(new NotifyUserCommand(
            conversation.CustomerId,
            NotificationType.ChatMessage,
            _localizer["Chat.NewMessageNotificationTitle"].Value,
            _localizer["Chat.NewMessageNotificationBody", command.AdminName].Value,
            "/Chat/Index"), ct);

        return Result<ChatMessageDto>.Success(dto);
    }
}
