using ECommerce.Application.DTOs.Chat;

namespace ECommerce.Application.Features.Chats.Queries.GetChatConversationForAdmin;

public class GetChatConversationForAdminQueryHandler : IRequestHandler<GetChatConversationForAdminQuery, Result<ChatConversationDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public GetChatConversationForAdminQueryHandler(IApplicationDbContext context, IStringLocalizer<SharedResource> localizer)
    {
        _context = context;
        _localizer = localizer;
    }

    public async Task<Result<ChatConversationDto>> Handle(GetChatConversationForAdminQuery request, CancellationToken ct)
    {
        var conversation = await _context.Set<ChatConversation>().FirstOrDefaultAsync(c => c.Id == request.ConversationId, ct);
        if (conversation is null)
        {
            return Result<ChatConversationDto>.Failure(_localizer["Chat.ConversationNotFound"].Value);
        }

        var dto = new ChatConversationDto
        {
            Id = conversation.Id,
            CustomerId = conversation.CustomerId,
            CustomerName = conversation.CustomerName,
            LastMessageAt = conversation.LastMessageAt,
            LastMessagePreview = conversation.LastMessagePreview,
            HasUnreadForAdmin = conversation.HasUnreadForAdmin,
            HasUnreadForCustomer = conversation.HasUnreadForCustomer
        };

        return Result<ChatConversationDto>.Success(dto);
    }
}
