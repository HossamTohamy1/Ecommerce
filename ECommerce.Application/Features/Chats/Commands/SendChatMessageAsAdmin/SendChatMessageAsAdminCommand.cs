using ECommerce.Application.DTOs.Chat;

namespace ECommerce.Application.Features.Chats.Commands.SendChatMessageAsAdmin;

public record SendChatMessageAsAdminCommand(Guid ConversationId, string AdminId, string AdminName, SendChatMessageRequest Request) : IRequest<Result<ChatMessageDto>>;
