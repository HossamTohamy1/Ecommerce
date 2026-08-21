using ECommerce.Application.DTOs.Chat;

namespace ECommerce.Application.Features.Chats.Queries.GetAllChatConversations;

public class GetAllChatConversationsQueryHandler : IRequestHandler<GetAllChatConversationsQuery, List<ChatConversationDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllChatConversationsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ChatConversationDto>> Handle(GetAllChatConversationsQuery request, CancellationToken ct)
    {
        return await _context.Set<ChatConversation>()
            .AsNoTracking()
            .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt)
            .Select(c => new ChatConversationDto
            {
                Id = c.Id,
                CustomerId = c.CustomerId,
                CustomerName = c.CustomerName,
                LastMessageAt = c.LastMessageAt,
                LastMessagePreview = c.LastMessagePreview,
                HasUnreadForAdmin = c.HasUnreadForAdmin,
                HasUnreadForCustomer = c.HasUnreadForCustomer
            })
            .ToListAsync(ct);
    }
}
