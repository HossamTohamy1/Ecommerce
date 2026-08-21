namespace ECommerce.Application.Features.Chats.Commands.MarkChatReadAsAdmin;

public record MarkChatReadAsAdminCommand(Guid ConversationId) : IRequest<Result>;
