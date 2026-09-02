using KPW.Application.DTOs.Messages;
using KPW.Application.Features.Pets;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.Messages.Commands;

public record SendMessageCommand(int PetId, SendMessageRequestDto Request) : IRequest<MessageDto>;

public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, MessageDto>
{
    private readonly DbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IChatNotificationService? _chatNotificationService;

    public SendMessageCommandHandler(
        DbContext dbContext,
        ICurrentUserService currentUserService,
        IChatNotificationService? chatNotificationService = null)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _chatNotificationService = chatNotificationService;
    }

    public async Task<MessageDto> Handle(SendMessageCommand command, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is null)
        {
            throw new UnauthorizedAccessException();
        }

        await PetAuthorization.EnsureCanAccessPet(
            _dbContext, _currentUserService, command.PetId, cancellationToken);

        var (ownerId, physioId) = await MessageThreadService.ResolveParticipantsAsync(
            _dbContext, _currentUserService, command.PetId, cancellationToken);

        if (command.Request.VideoSubmissionId is int videoSubmissionId)
        {
            var videoExists = await _dbContext.Set<VideoSubmission>()
                .AnyAsync(
                    v => v.VideoSubmissionId == videoSubmissionId && v.PetId == command.PetId,
                    cancellationToken);

            if (!videoExists)
            {
                throw new InvalidOperationException("Video submission not found for this pet.");
            }
        }

        var thread = await MessageThreadService.GetOrCreateThreadAsync(
            _dbContext, command.PetId, ownerId, physioId, cancellationToken);

        var message = new Message
        {
            MessageThreadId = thread.MessageThreadId,
            SenderUserId = _currentUserService.UserId.Value,
            Body = command.Request.Body.Trim(),
            VideoSubmissionId = command.Request.VideoSubmissionId,
            AttachmentUrl = command.Request.AttachmentUrl,
            AttachmentName = command.Request.AttachmentName,
            AttachmentType = command.Request.AttachmentType
        };

        _dbContext.Set<Message>().Add(message);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var created = await _dbContext.Set<Message>()
            .Include(m => m.Sender)
            .Include(m => m.VideoSubmission)
                .ThenInclude(v => v!.Exercise)
            .FirstAsync(m => m.MessageId == message.MessageId, cancellationToken);

        var dto = MessageMapper.ToDto(created);

        if (_chatNotificationService is not null)
        {
            await _chatNotificationService.NotifyMessageSentAsync(command.PetId, dto, cancellationToken);
        }

        return dto;
    }
}
