using KPW.Application.DTOs.Videos;
using KPW.Application.Features.Pets;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.Videos.Queries;

public record GetPetVideosQuery(int PetId) : IRequest<IReadOnlyList<VideoSubmissionDto>>;

public class GetPetVideosQueryHandler : IRequestHandler<GetPetVideosQuery, IReadOnlyList<VideoSubmissionDto>>
{
    private readonly DbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IVideoStorage _videoStorage;

    public GetPetVideosQueryHandler(
        DbContext dbContext,
        ICurrentUserService currentUserService,
        IVideoStorage videoStorage)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _videoStorage = videoStorage;
    }

    public async Task<IReadOnlyList<VideoSubmissionDto>> Handle(
        GetPetVideosQuery request,
        CancellationToken cancellationToken)
    {
        await PetAuthorization.EnsureCanAccessPet(
            _dbContext, _currentUserService, request.PetId, cancellationToken);

        var submissions = await _dbContext.Set<VideoSubmission>()
            .Include(v => v.Pet)
            .Include(v => v.Exercise)
            .Where(v => v.PetId == request.PetId)
            .OrderByDescending(v => v.CreatedDate)
            .ToListAsync(cancellationToken);

        return submissions.Select(v => new VideoSubmissionDto(
            v.VideoSubmissionId,
            v.PetId,
            v.Pet != null ? v.Pet.PetName : "Pet",
            v.ExerciseId,
            v.Exercise?.Title,
            v.Title,
            v.Notes,
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
