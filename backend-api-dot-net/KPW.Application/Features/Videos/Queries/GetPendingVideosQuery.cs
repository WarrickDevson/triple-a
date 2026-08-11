using KPW.Application.DTOs.Videos;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using KPW.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.Videos.Queries;

public record GetPendingVideosQuery : IRequest<IReadOnlyList<VideoSubmissionDto>>;

public class GetPendingVideosQueryHandler : IRequestHandler<GetPendingVideosQuery, IReadOnlyList<VideoSubmissionDto>>
{
    private readonly DbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IVideoStorage _videoStorage;

    public GetPendingVideosQueryHandler(
        DbContext dbContext,
        ICurrentUserService currentUserService,
        IVideoStorage videoStorage)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _videoStorage = videoStorage;
    }

    public async Task<IReadOnlyList<VideoSubmissionDto>> Handle(
        GetPendingVideosQuery request,
        CancellationToken cancellationToken)
    {
        if (_currentUserService.Role is not (UserRole.Physio or UserRole.SysAdmin))
        {
            throw new UnauthorizedAccessException();
        }

        var currentUser = await _dbContext.Set<User>()
            .AsNoTracking()
            .FirstAsync(u => u.UserId == _currentUserService.UserId, cancellationToken);

        if (currentUser.ClinicId is null)
        {
            return [];
        }

        var query = _dbContext.Set<VideoSubmission>()
            .Include(v => v.Pet)
            .Include(v => v.Exercise)
            .Where(v => !v.IsReviewed && v.Pet.Owner.ClinicId == currentUser.ClinicId);

        var submissions = await query
            .OrderByDescending(v => v.CreatedDate)
            .ToListAsync(cancellationToken);

        return submissions.Select(v => new VideoSubmissionDto(
            v.VideoSubmissionId,
            v.PetId,
            v.Pet.PetName,
            v.ExerciseId,
            v.Exercise.Title,
            _videoStorage.GetPublicUrl(v.RawVideoStorageUrl),
            v.ProcessedVideoStreamingUrl is not null
                ? _videoStorage.GetPublicUrl(v.ProcessedVideoStreamingUrl)
                : null,
            v.ProcessingStatus.ToString(),
            v.IsReviewed,
            v.PhysioFeedbackNotes,
            v.CreatedDate)).ToList();
    }
}
