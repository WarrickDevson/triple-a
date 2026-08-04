namespace KPW.Application.Interfaces;

public record AiChatSource(string Title, string Excerpt);

public record AiChatResult(string Answer, IReadOnlyList<AiChatSource> Sources);

public interface IAiChatService
{
    Task<AiChatResult> ChatAsync(string message, CancellationToken cancellationToken = default);
}
