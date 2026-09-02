using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using KPW.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.Videos.Commands;

public record DeleteVideoSubmissionCommand(int VideoSubmissionId) : IRequest<bool>;

public class DeleteVideoSubmissionCommandHandler : IRequestHandler<DeleteVideoSubmissionCommand, bool>
{
    private readonly DbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public DeleteVideoSubmissionCommandHandler(
        DbContext dbContext,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(DeleteVideoSubmissionCommand command, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var submission = await _dbContext.Set<VideoSubmission>()
            .Include(v => v.Pet)
            .FirstOrDefaultAsync(v => v.VideoSubmissionId == command.VideoSubmissionId, cancellationToken);

        if (submission is null)
        {
            throw new KeyNotFoundException("Video submission not found.");
        }

        var isOwner = submission.Pet?.OwnerId == _currentUserService.UserId.Value;
        var isPhysioOrAdmin = _currentUserService.Role is (UserRole.Physio or UserRole.SysAdmin);

        if (!isOwner && !isPhysioOrAdmin)
        {
            throw new UnauthorizedAccessException("You are not authorized to delete this video submission.");
        }

        submission.IsActive = false;
        submission.ModifiedDate = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
