using KPW.Application.DTOs.Videos;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using KPW.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.Videos.Commands;

public record ReviewVideoCommand(int VideoSubmissionId, ReviewVideoRequestDto Request) : IRequest<VideoSubmissionDto>;

public class ReviewVideoCommandHandler : IRequestHandler<ReviewVideoCommand, VideoSubmissionDto>
{
    private readonly DbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IVideoStorage _videoStorage;

    public ReviewVideoCommandHandler(
        DbContext dbContext,
        ICurrentUserService currentUserService,
        IVideoStorage videoStorage)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _videoStorage = videoStorage;
    }

    public async Task<VideoSubmissionDto> Handle(ReviewVideoCommand command, CancellationToken cancellationToken)
    {
        if (_currentUserService.Role is not (UserRole.Physio or UserRole.SysAdmin))
        {
            throw new UnauthorizedAccessException();
        }

        var submission = await _dbContext.Set<VideoSubmission>()
            .Include(v => v.Pet)
            .Include(v => v.Exercise)
            .FirstOrDefaultAsync(v => v.VideoSubmissionId == command.VideoSubmissionId, cancellationToken);

        if (submission is null)
        {
            throw new KeyNotFoundException("Video submission not found.");
        }

        submission.PhysioFeedbackNotes = command.Request.FeedbackNotes.Trim();
        submission.IsReviewed = true;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new VideoSubmissionDto(
            submission.VideoSubmissionId,
            submission.PetId,
            submission.Pet.PetName,
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
