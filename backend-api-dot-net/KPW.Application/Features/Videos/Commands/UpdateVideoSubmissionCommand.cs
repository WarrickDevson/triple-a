using KPW.Application.DTOs.Videos;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using KPW.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.Videos.Commands;

public record UpdateVideoSubmissionCommand(
    int VideoSubmissionId,
    UpdateVideoSubmissionRequestDto Request) : IRequest<VideoSubmissionDto>;

public class UpdateVideoSubmissionCommandHandler : IRequestHandler<UpdateVideoSubmissionCommand, VideoSubmissionDto>
{
    private readonly DbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IVideoStorage _videoStorage;

    public UpdateVideoSubmissionCommandHandler(
        DbContext dbContext,
        ICurrentUserService currentUserService,
        IVideoStorage videoStorage)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _videoStorage = videoStorage;
    }

    public async Task<VideoSubmissionDto> Handle(UpdateVideoSubmissionCommand command, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var submission = await _dbContext.Set<VideoSubmission>()
            .Include(v => v.Pet)
            .Include(v => v.Exercise)
            .FirstOrDefaultAsync(v => v.VideoSubmissionId == command.VideoSubmissionId, cancellationToken);

        if (submission is null)
        {
            throw new KeyNotFoundException("Video submission not found.");
        }

        var isOwner = submission.Pet?.OwnerId == _currentUserService.UserId.Value;
        var isPhysioOrAdmin = _currentUserService.Role is (UserRole.Physio or UserRole.SysAdmin);

        if (!isOwner && !isPhysioOrAdmin)
        {
            throw new UnauthorizedAccessException("You are not authorized to edit this video submission.");
        }

        var req = command.Request;
        submission.Title = string.IsNullOrWhiteSpace(req.Title) ? null : req.Title.Trim();
        submission.Notes = string.IsNullOrWhiteSpace(req.Notes) ? null : req.Notes.Trim();
        submission.ModifiedDate = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new VideoSubmissionDto(
            submission.VideoSubmissionId,
            submission.PetId,
            submission.Pet?.PetName ?? string.Empty,
            submission.ExerciseId,
            submission.Exercise?.Title,
            submission.Title,
            submission.Notes,
            _videoStorage.GetPublicUrl(submission.RawVideoStorageUrl),
            submission.ProcessedVideoStreamingUrl is not null
                ? _videoStorage.GetPublicUrl(submission.ProcessedVideoStreamingUrl)
                : null,
            submission.ProcessingStatus.ToString(),
            submission.IsReviewed,
            submission.PhysioFeedbackNotes,
            submission.CreatedDate);
    }
}
