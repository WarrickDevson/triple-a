using KPW.Api.Hubs;
using KPW.Application.DTOs.Messages;
using KPW.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace KPW.Api.Services;

public class ChatNotificationService : IChatNotificationService
{
    private readonly IHubContext<ChatHub> _hubContext;

    public ChatNotificationService(IHubContext<ChatHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyMessageSentAsync(int petId, MessageDto message, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.Group($"pet_{petId}").SendAsync("ReceiveMessage", message, cancellationToken);
        await _hubContext.Clients.All.SendAsync("ThreadUpdated", message, cancellationToken);
    }

    public async Task NotifyMessageReadAsync(int petId, int messageId, DateTime readAt, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.Group($"pet_{petId}").SendAsync("MessageRead", new { messageId, readAt }, cancellationToken);
    }
}
