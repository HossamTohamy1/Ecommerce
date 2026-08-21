namespace ECommerce.Application.Features.Chats.Commands.MarkChatReadAsCustomer;

public record MarkChatReadAsCustomerCommand(string CustomerId) : IRequest<Result>;
