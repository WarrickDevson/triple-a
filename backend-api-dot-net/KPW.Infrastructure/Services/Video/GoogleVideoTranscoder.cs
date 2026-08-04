using Google.Cloud.Video.Transcoder.V1;
using KPW.Application.Features.Videos.Commands;
using KPW.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KPW.Infrastructure.Services.Video;

public class GoogleVideoTranscoder : IVideoTranscoder
{
    private readonly TranscoderServiceClient _client;
    private readonly VideoOptions _options;
    private readonly GcsVideoStorage _gcsStorage;
    private readonly ILogger<GoogleVideoTranscoder> _logger;

    public GoogleVideoTranscoder(
        IOptions<VideoOptions> options,
        GcsVideoStorage gcsStorage,
        ILogger<GoogleVideoTranscoder> logger)
    {
        _options = options.Value;
        _gcsStorage = gcsStorage;
        _logger = logger;

        if (string.IsNullOrWhiteSpace(_options.ProjectId) || string.IsNullOrWhiteSpace(_options.Location))
        {
            throw new InvalidOperationException("Video:ProjectId and Video:Location are required when Video:Provider is Google.");
        }

        _client = new TranscoderServiceClientBuilder
        {
            Endpoint = $"{_options.Location}-video.googleapis.com"
        }.Build();
    }

    public async Task<string> TranscodeAsync(string rawStoragePath, CancellationToken cancellationToken = default)
    {
        var inputUri = _gcsStorage.GetGsUri(rawStoragePath);
        var outputFolder = $"videos/processed/{Guid.NewGuid():N}/";
        var outputUri = $"gs://{_options.Bucket}/{outputFolder}";

        var parent = $"projects/{_options.ProjectId}/locations/{_options.Location}";

        var job = new Job
        {
            InputUri = inputUri,
            OutputUri = outputUri,
            Config = new JobConfig
            {
                ElementaryStreams =
                {
                    new ElementaryStream
                    {
                        Key = "video-stream0",
                        VideoStream = new VideoStream
                        {
                            H264 = new VideoStream.Types.H264CodecSettings
                            {
                                BitrateBps = 2_500_000,
                                FrameRate = 30,
                                HeightPixels = 720,
                                WidthPixels = 1280
                            }
                        }
                    },
                    new ElementaryStream
                    {
                        Key = "audio-stream0",
                        AudioStream = new AudioStream
                        {
                            Codec = "aac",
                            BitrateBps = 128_000
                        }
                    }
                },
                MuxStreams =
                {
                    new MuxStream
                    {
                        Key = "sd",
                        Container = "mp4",
                        ElementaryStreams = { "video-stream0", "audio-stream0" }
                    }
                }
            }
        };

        _logger.LogInformation("Creating transcoder job for {InputUri} -> {OutputUri}", inputUri, outputUri);

        var createdJob = await _client.CreateJobAsync(parent, job, cancellationToken: cancellationToken);
        var jobName = createdJob.Name;

        var deadline = DateTime.UtcNow.AddMinutes(30);
        while (DateTime.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var currentJob = await _client.GetJobAsync(jobName, cancellationToken: cancellationToken);

            if (currentJob.State == Job.Types.ProcessingState.Succeeded)
            {
                var processedObject = $"{outputFolder}sd.mp4";
                _logger.LogInformation("Transcoder job succeeded. Output object: {Object}", processedObject);
                return processedObject;
            }

            if (currentJob.State == Job.Types.ProcessingState.Failed)
            {
                throw new InvalidOperationException($"Transcoder job failed for {inputUri}.");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
        }

        throw new TimeoutException($"Transcoder job timed out for {inputUri}.");
    }
}
