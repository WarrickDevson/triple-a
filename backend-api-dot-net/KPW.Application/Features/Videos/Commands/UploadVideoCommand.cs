using KPW.Application.DTOs.Videos;
using KPW.Application.Features.Pets;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using KPW.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.Videos.Commands;

public record UploadVideoCommand(
    int PetId,
    int ExerciseId,
    Stream FileStream,
    string FileName,
    string ContentType,
    long FileSize) : IRequest<UploadVideoResultDto>;

public class UploadVideoCommandHandler : IRequestHandler<UploadVideoCommand, UploadVideoResultDto>
{
    private static readonly HashSet<string> AllowedExtensions = [".mp4", ".mov", ".hevc"];
    private static readonly HashSet<string> AllowedContentTypes =
    [
        "video/mp4",
        "video/quicktime",
        "video/hevc",
        "application/octet-stream"
    ];

    private readonly DbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IVideoStorage _videoStorage;
    private readonly IVideoProcessingQueue _processingQueue;
    private readonly long _maxBytes;

    public UploadVideoCommandHandler(
        DbContext dbContext,
        ICurrentUserService currentUserService,
        IVideoStorage videoStorage,
        IVideoProcessingQueue processingQueue,
        Microsoft.Extensions.Options.IOptions<VideoOptions> videoOptions)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _videoStorage = videoStorage;
        _processingQueue = processingQueue;
        _maxBytes = videoOptions.Value.MaxBytes;
    }

    public async Task<UploadVideoResultDto> Handle(UploadVideoCommand command, CancellationToken cancellationToken)
    {
        if (_currentUserService.Role != UserRole.Owner)
        {
            throw new UnauthorizedAccessException("Only pet owners can upload exercise videos.");
        }

        await PetAuthorization.EnsureCanAccessPet(
            _dbContext, _currentUserService, command.PetId, cancellationToken);

        if (command.FileSize <= 0 || command.FileSize > _maxBytes)
        {
            throw new InvalidOperationException($"Video must be between 1 byte and {_maxBytes / (1024 * 1024)}MB.");
        }

        var extension = Path.GetExtension(command.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException("Only .mp4, .mov, and .hevc video files are allowed.");
        }

        if (!string.IsNullOrWhiteSpace(command.ContentType) &&
            !AllowedContentTypes.Contains(command.ContentType.ToLowerInvariant()))
        {
            throw new InvalidOperationException("Unsupported video content type.");
        }

        var exerciseExists = await _dbContext.Set<Exercise>()
            .AnyAsync(e => e.ExerciseId == command.ExerciseId, cancellationToken);
        if (!exerciseExists)
        {
            throw new KeyNotFoundException("Exercise not found.");
        }

        var storagePath = await _videoStorage.UploadAsync(
            command.FileStream, command.FileName, cancellationToken);

        var submission = new VideoSubmission
        {
            PetId = command.PetId,
            ExerciseId = command.ExerciseId,
            RawVideoStorageUrl = storagePath,
            ProcessingStatus = VideoProcessingStatus.Pending,
            IsReviewed = false
        };

        _dbContext.Set<VideoSubmission>().Add(submission);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _processingQueue.EnqueueAsync(submission.VideoSubmissionId, cancellationToken);

        return new UploadVideoResultDto(
            submission.VideoSubmissionId,
            submission.ProcessingStatus.ToString(),
            _videoStorage.GetPublicUrl(storagePath));
    }
}

public class VideoOptions
{
    public const string SectionName = "Video";

    public string Provider { get; set; } = "Local";
    public string LocalRoot { get; set; } = "wwwroot/uploads";
    public long MaxBytes { get; set; } = 104_857_600;
    public string ProjectId { get; set; } = string.Empty;
    public string Bucket { get; set; } = string.Empty;
    public string Location { get; set; } = "us-central1";
    public int SignedUrlMinutes { get; set; } = 60;
}
