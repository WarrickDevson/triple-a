namespace KPW.Application.Interfaces;

public interface IVideoProcessingQueue
{
    ValueTask EnqueueAsync(int videoSubmissionId, CancellationToken cancellationToken = default);
    IAsyncEnumerable<int> DequeueAllAsync(CancellationToken cancellationToken);
}
