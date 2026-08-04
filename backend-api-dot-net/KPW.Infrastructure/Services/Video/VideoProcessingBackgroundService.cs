using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using KPW.Domain.Enums;
using KPW.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KPW.Infrastructure.Services.Video;

public class VideoProcessingBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IVideoProcessingQueue _queue;
    private readonly ILogger<VideoProcessingBackgroundService> _logger;

    public VideoProcessingBackgroundService(
        IServiceScopeFactory scopeFactory,
        IVideoProcessingQueue queue,
        ILogger<VideoProcessingBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _queue = queue;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var submissionId in _queue.DequeueAllAsync(stoppingToken))
        {
            try
            {
                await ProcessSubmissionAsync(submissionId, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process video submission {SubmissionId}", submissionId);
            }
        }
    }

    private async Task ProcessSubmissionAsync(int submissionId, CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var transcoder = scope.ServiceProvider.GetRequiredService<IVideoTranscoder>();

        var submission = await dbContext.VideoSubmissions
            .FirstOrDefaultAsync(v => v.VideoSubmissionId == submissionId, cancellationToken);

        if (submission is null)
        {
            return;
        }

        submission.ProcessingStatus = VideoProcessingStatus.Processing;
        await dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            var processedPath = await transcoder.TranscodeAsync(submission.RawVideoStorageUrl, cancellationToken);
            submission.ProcessedVideoStreamingUrl = processedPath;
            submission.ProcessingStatus = VideoProcessingStatus.Ready;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transcoding failed for submission {SubmissionId}", submissionId);
            submission.ProcessingStatus = VideoProcessingStatus.Failed;
            submission.ProcessedVideoStreamingUrl = submission.RawVideoStorageUrl;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation(
            "Video submission {SubmissionId} processed with status {Status}",
            submissionId,
            submission.ProcessingStatus);
    }
}
