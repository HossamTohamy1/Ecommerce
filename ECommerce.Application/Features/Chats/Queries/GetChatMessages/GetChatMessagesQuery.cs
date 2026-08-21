using ECommerce.Application.DTOs.Chat;

namespace ECommerce.Application.Features.Chats.Queries.GetChatMessages;

public record GetChatMessagesQuery(Guid ConversationId, int Page, int PageSize) : IRequest<PagedResult<ChatMessageDto>>;
