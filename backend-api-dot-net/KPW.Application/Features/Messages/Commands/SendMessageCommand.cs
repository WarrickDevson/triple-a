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

    public SendMessageCommandHandler(DbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
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
            VideoSubmissionId = command.Request.VideoSubmissionId
        };

        _dbContext.Set<Message>().Add(message);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var created = await _dbContext.Set<Message>()
            .Include(m => m.Sender)
            .FirstAsync(m => m.MessageId == message.MessageId, cancellationToken);

        return MessageMapper.ToDto(created);
    }
}
