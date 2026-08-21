using System.Security.Claims;
using ECommerce.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ECommerce.Infrastructure.Realtime;

[Authorize]
public class NotificationHub : Hub
{
    public const string AdminsGroup = "admins";

    public static string UserGroup(string userId) => $"user:{userId}";

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, UserGroup(userId));

            if (Context.User!.IsInRole(AppConstants.Roles.Admin))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, AdminsGroup);
            }
        }

        await base.OnConnectedAsync();
    }
}
