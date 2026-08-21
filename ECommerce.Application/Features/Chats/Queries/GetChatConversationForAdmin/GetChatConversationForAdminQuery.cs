using ECommerce.Application.DTOs.Chat;

namespace ECommerce.Application.Features.Chats.Queries.GetChatConversationForAdmin;

public record GetChatConversationForAdminQuery(Guid ConversationId) : IRequest<Result<ChatConversationDto>>;
