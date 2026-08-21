namespace ECommerce.Application.Features.Chats.Commands.MarkChatReadAsCustomer;

public class MarkChatReadAsCustomerCommandHandler : IRequestHandler<MarkChatReadAsCustomerCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public MarkChatReadAsCustomerCommandHandler(IApplicationDbContext context, IStringLocalizer<SharedResource> localizer)
    {
        _context = context;
        _localizer = localizer;
    }

    public async Task<Result> Handle(MarkChatReadAsCustomerCommand command, CancellationToken ct)
    {
        var conversation = await _context.Set<ChatConversation>().FirstOrDefaultAsync(c => c.CustomerId == command.CustomerId, ct);
        if (conversation is null)
        {
            return Result.Failure(_localizer["Chat.ConversationNotFound"].Value);
        }

        if (conversation.HasUnreadForCustomer)
        {
            conversation.HasUnreadForCustomer = false;
            var unread = await _context.Set<ChatMessage>()
                .Where(m => m.ConversationId == conversation.Id && m.SenderRole == ChatSenderRole.Admin && !m.IsRead)
                .ToListAsync(ct);

            foreach (var message in unread)
            {
                message.IsRead = true;
            }

            await _context.SaveChangesAsync(ct);
        }

        return Result.Success();
    }
}
