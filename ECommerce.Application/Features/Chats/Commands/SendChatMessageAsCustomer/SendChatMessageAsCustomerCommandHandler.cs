using ECommerce.Application.DTOs.Chat;
using ECommerce.Application.Features.Notifications.Commands.NotifyAdmins;

namespace ECommerce.Application.Features.Chats.Commands.SendChatMessageAsCustomer;

public class SendChatMessageAsCustomerCommandHandler : IRequestHandler<SendChatMessageAsCustomerCommand, Result<ChatMessageDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IRealtimeNotifier _realtimeNotifier;
    private readonly IMediator _mediator;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public SendChatMessageAsCustomerCommandHandler(
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

    public async Task<Result<ChatMessageDto>> Handle(SendChatMessageAsCustomerCommand command, CancellationToken ct)
    {
        var conversation = await _context.Set<ChatConversation>().FirstOrDefaultAsync(c => c.CustomerId == command.CustomerId, ct);
        if (conversation is null)
        {
            conversation = new ChatConversation
            {
                CustomerId = command.CustomerId,
                CustomerName = command.CustomerName,
                CreatedById = command.CustomerId
            };
            _context.Set<ChatConversation>().Add(conversation);
        }

        var message = new ChatMessage
        {
            Conversation = conversation,
            SenderId = command.CustomerId,
            SenderName = command.CustomerName,
            SenderRole = ChatSenderRole.Customer,
            Content = command.Request.Content.Trim(),
            CreatedById = command.CustomerId
        };

        _context.Set<ChatMessage>().Add(message);

        conversation.LastMessageAt = DateTime.UtcNow;
        conversation.LastMessagePreview = message.Content.Length <= 150 ? message.Content : message.Content[..150] + "...";
        conversation.HasUnreadForAdmin = true;
        conversation.UpdatedAt = DateTime.UtcNow;
        conversation.UpdatedById = command.CustomerId;

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
        await _mediator.Send(new NotifyAdminsCommand(
            NotificationType.ChatMessage,
            _localizer["Chat.NewMessageNotificationTitle"].Value,
            _localizer["Chat.NewMessageNotificationBody", command.CustomerName].Value,
            $"/Chat/Admin/Index?conversationId={conversation.Id}"), ct);

        return Result<ChatMessageDto>.Success(dto);
    }
}
