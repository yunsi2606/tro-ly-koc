using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace TroLiKOC.API.Hubs;

[Authorize]
public class JobHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        }
        await base.OnConnectedAsync();
    }
}
