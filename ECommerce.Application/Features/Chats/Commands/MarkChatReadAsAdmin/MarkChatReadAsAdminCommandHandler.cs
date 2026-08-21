namespace ECommerce.Application.Features.Chats.Commands.MarkChatReadAsAdmin;

public class MarkChatReadAsAdminCommandHandler : IRequestHandler<MarkChatReadAsAdminCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public MarkChatReadAsAdminCommandHandler(IApplicationDbContext context, IStringLocalizer<SharedResource> localizer)
    {
        _context = context;
        _localizer = localizer;
    }

    public async Task<Result> Handle(MarkChatReadAsAdminCommand command, CancellationToken ct)
    {
        var conversation = await _context.Set<ChatConversation>().FirstOrDefaultAsync(c => c.Id == command.ConversationId, ct);
        if (conversation is null)
        {
            return Result.Failure(_localizer["Chat.ConversationNotFound"].Value);
        }

        if (conversation.HasUnreadForAdmin)
        {
            conversation.HasUnreadForAdmin = false;
            var unread = await _context.Set<ChatMessage>()
                .Where(m => m.ConversationId == conversation.Id && m.SenderRole == ChatSenderRole.Customer && !m.IsRead)
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
