using System.Security.Claims;
using ECommerce.Application.DTOs.Chat;
using ECommerce.Application.Features.Chats.Commands.SendChatMessageAsAdmin;
using ECommerce.Application.Features.Chats.Commands.SendChatMessageAsCustomer;
using ECommerce.Application.Features.Chats.Queries.GetOrCreateChatForCustomer;
using ECommerce.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ECommerce.Infrastructure.Realtime;

[Authorize]
public class ChatHub : Hub
{
    private readonly IMediator _mediator;

    public ChatHub(IMediator mediator)
    {
        _mediator = mediator;
    }

    public static string ConversationGroup(Guid conversationId) => $"chat:{conversationId}";
    public const string AdminsGroup = "chat:admins";

    public override async Task OnConnectedAsync()
    {
        var user = Context.User;
        var userId = user?.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!string.IsNullOrEmpty(userId) && user is not null)
        {
            if (user.IsInRole(AppConstants.Roles.Admin))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, AdminsGroup);
            }
            else
            {
                var name = user.FindFirstValue(ClaimTypes.Name) ?? user.FindFirstValue(ClaimTypes.Email) ?? "Customer";
                var conversation = await _mediator.Send(new GetOrCreateChatForCustomerQuery(userId, name));
                await Groups.AddToGroupAsync(Context.ConnectionId, ConversationGroup(conversation.Id));
            }
        }

        await base.OnConnectedAsync();
    }

    [Authorize(Roles = AppConstants.Roles.Admin)]
    public Task JoinConversation(Guid conversationId)
        => Groups.AddToGroupAsync(Context.ConnectionId, ConversationGroup(conversationId));

    [Authorize(Roles = AppConstants.Roles.Admin)]
    public Task LeaveConversation(Guid conversationId)
        => Groups.RemoveFromGroupAsync(Context.ConnectionId, ConversationGroup(conversationId));

    public async Task SendMessage(Guid? conversationId, string content)
    {
        if (string.IsNullOrWhiteSpace(content) || content.Length > 2000)
        {
            return;
        }

        var user = Context.User!;
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return;
        }

        var name = user.FindFirstValue(ClaimTypes.Name) ?? user.FindFirstValue(ClaimTypes.Email) ?? "User";
        var request = new SendChatMessageRequest { Content = content };

        if (user.IsInRole(AppConstants.Roles.Admin))
        {
            if (conversationId is null)
            {
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, ConversationGroup(conversationId.Value));
            await _mediator.Send(new SendChatMessageAsAdminCommand(conversationId.Value, userId, name, request));
        }
        else
        {
            var result = await _mediator.Send(new SendChatMessageAsCustomerCommand(userId, name, request));
            if (result.Succeeded && result.Data is not null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, ConversationGroup(result.Data.ConversationId));
            }
        }
    }
}

