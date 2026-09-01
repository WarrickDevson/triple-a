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
    int? ExerciseId,
    string? Title,
    string? Notes,
    Stream FileStream,
    string FileName,
    string ContentType,
    long FileSize) : IRequest<UploadVideoResultDto>;

public class UploadVideoCommandHandler : IRequestHandler<UploadVideoCommand, UploadVideoResultDto>
{
    private static readonly HashSet<string> AllowedExtensions =
    [
        ".mp4", ".mov", ".hevc", ".m4v", ".webm", ".mkv", ".3gp", ".avi"
    ];
    private static readonly HashSet<string> AllowedContentTypes =
    [
        "video/mp4",
        "video/quicktime",
        "video/hevc",
        "video/x-m4v",
        "video/webm",
        "video/x-matroska",
        "video/3gpp",
        "video/avi",
        "application/octet-stream"
    ];

    private readonly DbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IVideoStorage _videoStorage;
    private readonly IVideoProcessingQueue _processingQueue;
    private const long MaxBytes = 104_857_600; // 100MB

    public UploadVideoCommandHandler(
        DbContext dbContext,
        ICurrentUserService currentUserService,
        IVideoStorage videoStorage,
        IVideoProcessingQueue processingQueue)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _videoStorage = videoStorage;
        _processingQueue = processingQueue;
    }

    public async Task<UploadVideoResultDto> Handle(UploadVideoCommand command, CancellationToken cancellationToken)
    {
        if (_currentUserService.Role != UserRole.Owner)
        {
            throw new UnauthorizedAccessException("Only pet owners can upload exercise videos.");
        }

        await PetAuthorization.EnsureCanAccessPet(
            _dbContext, _currentUserService, command.PetId, cancellationToken);

        if (command.FileSize <= 0 || command.FileSize > MaxBytes)
        {
            throw new InvalidOperationException($"Video must be between 1 byte and {MaxBytes / (1024 * 1024)}MB.");
        }

        var extension = Path.GetExtension(command.FileName).ToLowerInvariant();
        if (!string.IsNullOrEmpty(extension) && !AllowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException("Unsupported video file format.");
        }

        if (!string.IsNullOrWhiteSpace(command.ContentType) &&
            !command.ContentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase) &&
            !AllowedContentTypes.Contains(command.ContentType.ToLowerInvariant()))
        {
            throw new InvalidOperationException("Unsupported video content type.");
        }

        if (command.ExerciseId.HasValue)
        {
            var exerciseExists = await _dbContext.Set<Exercise>()
                .AnyAsync(e => e.ExerciseId == command.ExerciseId.Value, cancellationToken);
            if (!exerciseExists)
            {
                throw new KeyNotFoundException("Exercise not found.");
            }
        }

        var storagePath = await _videoStorage.UploadAsync(
            command.FileStream, command.FileName, cancellationToken);

        var submission = new VideoSubmission
        {
            PetId = command.PetId,
            ExerciseId = command.ExerciseId,
            Title = string.IsNullOrWhiteSpace(command.Title) ? null : command.Title.Trim(),
            Notes = string.IsNullOrWhiteSpace(command.Notes) ? null : command.Notes.Trim(),
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
