using ECommerce.Application.DTOs.Chat;

namespace ECommerce.Application.Features.Chats.Queries.GetOrCreateChatForCustomer;

public record GetOrCreateChatForCustomerQuery(string CustomerId, string CustomerName) : IRequest<ChatConversationDto>;
