using KPW.Application.DTOs.Messages;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.Messages.Commands;

public record MarkMessageReadCommand(int MessageId) : IRequest<MessageDto>;

public class MarkMessageReadCommandHandler : IRequestHandler<MarkMessageReadCommand, MessageDto>
{
    private readonly DbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IChatNotificationService? _chatNotificationService;

    public MarkMessageReadCommandHandler(
        DbContext dbContext,
        ICurrentUserService currentUserService,
        IChatNotificationService? chatNotificationService = null)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _chatNotificationService = chatNotificationService;
    }

    public async Task<MessageDto> Handle(MarkMessageReadCommand command, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is null)
        {
            throw new UnauthorizedAccessException();
        }

        var message = await _dbContext.Set<Message>()
            .Include(m => m.Sender)
            .Include(m => m.Thread)
            .Include(m => m.VideoSubmission)
                .ThenInclude(v => v!.Exercise)
            .FirstOrDefaultAsync(m => m.MessageId == command.MessageId, cancellationToken);

        if (message is null)
        {
            throw new KeyNotFoundException("Message not found.");
        }

        if (message.SenderUserId == _currentUserService.UserId)
        {
            throw new InvalidOperationException("You cannot mark your own message as read.");
        }

        var isParticipant = message.Thread.OwnerId == _currentUserService.UserId ||
                            message.Thread.PhysioId == _currentUserService.UserId;

        if (!isParticipant)
        {
            throw new UnauthorizedAccessException();
        }

        message.ReadAt ??= DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        var dto = MessageMapper.ToDto(message);

        if (_chatNotificationService is not null && message.ReadAt.HasValue)
        {
            await _chatNotificationService.NotifyMessageReadAsync(
                message.Thread.PetId, message.MessageId, message.ReadAt.Value, cancellationToken);
        }

        return dto;
    }
}
