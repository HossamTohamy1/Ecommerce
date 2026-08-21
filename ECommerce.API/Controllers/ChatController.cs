using System.Security.Claims;
using ECommerce.Application.DTOs.Chat;
using ECommerce.Application.Features.Chats.Commands.MarkChatReadAsAdmin;
using ECommerce.Application.Features.Chats.Commands.MarkChatReadAsCustomer;
using ECommerce.Application.Features.Chats.Commands.SendChatMessageAsAdmin;
using ECommerce.Application.Features.Chats.Commands.SendChatMessageAsCustomer;
using ECommerce.Application.Features.Chats.Queries.GetAllChatConversations;
using ECommerce.Application.Features.Chats.Queries.GetChatMessages;
using ECommerce.Application.Features.Chats.Queries.GetOrCreateChatForCustomer;

namespace ECommerce.API.Controllers;

[Route("api/chat")]
[Authorize]
public class ChatController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public ChatController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private string CurrentUserName => User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.Email) ?? "User";

    [HttpGet("my")]
    public async Task<IActionResult> GetMyConversation(CancellationToken ct)
        => Ok(await _mediator.Send(new GetOrCreateChatForCustomerQuery(CurrentUserId, CurrentUserName), ct));

    [HttpGet("conversations")]
    [Authorize(Roles = AppConstants.Roles.Admin)]
    public async Task<IActionResult> GetAllConversations(CancellationToken ct)
        => Ok(await _mediator.Send(new GetAllChatConversationsQuery(), ct));

    [HttpGet("{conversationId:guid}/messages")]
    public async Task<IActionResult> GetMessages(Guid conversationId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken ct = default)
    {
        var authorized = await CanAccessConversationAsync(conversationId, ct);
        if (!authorized)
        {
            return Forbid();
        }

        return Ok(await _mediator.Send(new GetChatMessagesQuery(conversationId, page, pageSize), ct));
    }

    [HttpPost("{conversationId:guid}/messages")]
    public async Task<IActionResult> SendMessage(Guid conversationId, [FromBody] SendChatMessageRequest request, CancellationToken ct)
    {
        var isAdmin = User.IsInRole(AppConstants.Roles.Admin);
        if (!isAdmin)
        {
            var authorized = await CanAccessConversationAsync(conversationId, ct);
            if (!authorized)
            {
                return Forbid();
            }
        }

        var result = isAdmin
            ? await _mediator.Send(new SendChatMessageAsAdminCommand(conversationId, CurrentUserId, CurrentUserName, request), ct)
            : await _mediator.Send(new SendChatMessageAsCustomerCommand(CurrentUserId, CurrentUserName, request), ct);

        return result.Succeeded ? Ok(result.Data) : BadRequest(new { message = result.Error });
    }

    [HttpPut("{conversationId:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid conversationId, CancellationToken ct)
    {
        Result result;
        if (User.IsInRole(AppConstants.Roles.Admin))
        {
            result = await _mediator.Send(new MarkChatReadAsAdminCommand(conversationId), ct);
        }
        else
        {
            var authorized = await CanAccessConversationAsync(conversationId, ct);
            if (!authorized)
            {
                return Forbid();
            }

            result = await _mediator.Send(new MarkChatReadAsCustomerCommand(CurrentUserId), ct);
        }

        return result.Succeeded ? Ok() : NotFound(new { message = result.Error });
    }

    private async Task<bool> CanAccessConversationAsync(Guid conversationId, CancellationToken ct)
    {
        if (User.IsInRole(AppConstants.Roles.Admin))
        {
            return true;
        }

        var mine = await _mediator.Send(new GetOrCreateChatForCustomerQuery(CurrentUserId, CurrentUserName), ct);
        return mine.Id == conversationId;
    }
}

