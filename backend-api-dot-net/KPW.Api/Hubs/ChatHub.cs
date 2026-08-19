using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace KPW.Api.Hubs;

[Authorize]
public class ChatHub : Hub
{
    public async Task JoinPetThread(int petId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"pet_{petId}");
    }

    public async Task LeavePetThread(int petId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"pet_{petId}");
    }
}
