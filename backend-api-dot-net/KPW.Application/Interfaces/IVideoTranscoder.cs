namespace KPW.Application.Interfaces;

public interface IVideoTranscoder
{
    Task<string> TranscodeAsync(string rawStoragePath, CancellationToken cancellationToken = default);
}
