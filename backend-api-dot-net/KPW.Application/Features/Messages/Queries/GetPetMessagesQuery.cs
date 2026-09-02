using KPW.Application.DTOs.Messages;
using KPW.Application.Features.Pets;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.Messages.Queries;

public record GetPetMessagesQuery(int PetId) : IRequest<IReadOnlyList<MessageDto>>;

public class GetPetMessagesQueryHandler : IRequestHandler<GetPetMessagesQuery, IReadOnlyList<MessageDto>>
{
    private readonly DbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetPetMessagesQueryHandler(DbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<MessageDto>> Handle(
        GetPetMessagesQuery request,
        CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is null)
        {
            throw new UnauthorizedAccessException();
        }

        await PetAuthorization.EnsureCanAccessPet(
            _dbContext, _currentUserService, request.PetId, cancellationToken);

        var thread = await _dbContext.Set<MessageThread>()
            .Include(t => t.Messages)
                .ThenInclude(m => m.Sender)
            .Include(t => t.Messages)
                .ThenInclude(m => m.VideoSubmission)
                    .ThenInclude(v => v!.Exercise)
            .FirstOrDefaultAsync(t => t.PetId == request.PetId, cancellationToken);

        if (thread is null)
        {
            return [];
        }

        var currentUserId = _currentUserService.UserId.Value;
        var unreadMessages = thread.Messages
            .Where(m => m.SenderUserId != currentUserId && m.ReadAt is null)
            .ToList();

        foreach (var message in unreadMessages)
        {
            message.ReadAt = DateTime.UtcNow;
        }

        if (unreadMessages.Count > 0)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return thread.Messages
            .OrderBy(m => m.CreatedDate)
            .Select(MessageMapper.ToDto)
            .ToList();
    }
}
