using ECommerce.Application.DTOs.Chat;

namespace ECommerce.Application.Features.Chats.Queries.GetOrCreateChatForCustomer;

public class GetOrCreateChatForCustomerQueryHandler : IRequestHandler<GetOrCreateChatForCustomerQuery, ChatConversationDto>
{
    private readonly IApplicationDbContext _context;

    public GetOrCreateChatForCustomerQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ChatConversationDto> Handle(GetOrCreateChatForCustomerQuery request, CancellationToken ct)
    {
        var conversation = await _context.Set<ChatConversation>().FirstOrDefaultAsync(c => c.CustomerId == request.CustomerId, ct);
        if (conversation is null)
        {
            conversation = new ChatConversation
            {
                CustomerId = request.CustomerId,
                CustomerName = request.CustomerName,
                CreatedById = request.CustomerId
            };
            _context.Set<ChatConversation>().Add(conversation);
            await _context.SaveChangesAsync(ct);
        }

        return new ChatConversationDto
        {
            Id = conversation.Id,
            CustomerId = conversation.CustomerId,
            CustomerName = conversation.CustomerName,
            LastMessageAt = conversation.LastMessageAt,
            LastMessagePreview = conversation.LastMessagePreview,
            HasUnreadForAdmin = conversation.HasUnreadForAdmin,
            HasUnreadForCustomer = conversation.HasUnreadForCustomer
        };
    }
}
