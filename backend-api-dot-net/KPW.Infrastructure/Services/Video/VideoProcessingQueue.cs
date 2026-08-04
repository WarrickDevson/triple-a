using System.Threading.Channels;
using KPW.Application.Interfaces;

namespace KPW.Infrastructure.Services.Video;

public class VideoProcessingQueue : IVideoProcessingQueue
{
    private readonly Channel<int> _channel = Channel.CreateUnbounded<int>(
        new UnboundedChannelOptions { SingleReader = true });

    public ValueTask EnqueueAsync(int videoSubmissionId, CancellationToken cancellationToken = default) =>
        _channel.Writer.WriteAsync(videoSubmissionId, cancellationToken);

    public async IAsyncEnumerable<int> DequeueAllAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var id in _channel.Reader.ReadAllAsync(cancellationToken))
        {
            yield return id;
        }
    }
}
