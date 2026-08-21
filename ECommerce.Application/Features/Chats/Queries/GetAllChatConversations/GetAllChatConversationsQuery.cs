using ECommerce.Application.DTOs.Chat;

namespace ECommerce.Application.Features.Chats.Queries.GetAllChatConversations;

public record GetAllChatConversationsQuery : IRequest<List<ChatConversationDto>>;
