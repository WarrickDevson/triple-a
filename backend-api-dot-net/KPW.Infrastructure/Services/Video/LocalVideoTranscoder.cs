using System.Diagnostics;
using KPW.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace KPW.Infrastructure.Services.Video;

public class LocalVideoTranscoder : IVideoTranscoder
{
    private readonly LocalVideoStorage _storage;
    private readonly ILogger<LocalVideoTranscoder> _logger;

    public LocalVideoTranscoder(LocalVideoStorage storage, ILogger<LocalVideoTranscoder> logger)
    {
        _storage = storage;
        _logger = logger;
    }

    public async Task<string> TranscodeAsync(string rawStoragePath, CancellationToken cancellationToken = default)
    {
        var inputPath = _storage.GetFullPath(rawStoragePath);
        if (!File.Exists(inputPath))
        {
            throw new FileNotFoundException("Raw video file not found.", inputPath);
        }

        var outputRelative = $"videos/processed_{Guid.NewGuid():N}.mp4";
        var outputPath = _storage.GetFullPath(outputRelative);

        if (await TryFfmpegTranscodeAsync(inputPath, outputPath, cancellationToken))
        {
            return outputRelative;
        }

        _logger.LogWarning("FFmpeg unavailable or failed; copying raw file as streamable output.");
        File.Copy(inputPath, outputPath, overwrite: true);
        return outputRelative;
    }

    private async Task<bool> TryFfmpegTranscodeAsync(
        string inputPath,
        string outputPath,
        CancellationToken cancellationToken)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-y -i \"{inputPath}\" -c:v libx264 -preset fast -crf 23 -c:a aac -movflags +faststart \"{outputPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process is null)
            {
                return false;
            }

            await process.WaitForExitAsync(cancellationToken);
            return process.ExitCode == 0 && File.Exists(outputPath);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "FFmpeg transcode attempt failed.");
            return false;
        }
    }
}
