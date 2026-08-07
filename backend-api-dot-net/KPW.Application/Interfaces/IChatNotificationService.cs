using KPW.Application.DTOs.Messages;

namespace KPW.Application.Interfaces;

public interface IChatNotificationService
{
    Task NotifyMessageSentAsync(int petId, MessageDto message, CancellationToken cancellationToken = default);
    Task NotifyMessageReadAsync(int petId, int messageId, DateTime readAt, CancellationToken cancellationToken = default);
}
