using ECommerce.Application.DTOs.Chat;

namespace ECommerce.Application.Features.Chats.Commands.SendChatMessageAsCustomer;

public record SendChatMessageAsCustomerCommand(string CustomerId, string CustomerName, SendChatMessageRequest Request) : IRequest<Result<ChatMessageDto>>;
