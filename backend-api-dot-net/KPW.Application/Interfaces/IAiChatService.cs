namespace KPW.Application.Interfaces;

public record AiChatSource(string Title, string Excerpt);

public record AiChatResult(string Answer, IReadOnlyList<AiChatSource> Sources);

public record AiChatAttachment(
    string? Url,
    string? Base64Data,
    string? MimeType,
    string? FileName);

public record AiChatHistoryTurn(string Role, string Content);

public interface IAiChatService
{
    Task<AiChatResult> ChatAsync(
        string message,
        string? clinicalContext = null,
        AiChatAttachment? attachment = null,
        IReadOnlyList<AiChatHistoryTurn>? history = null,
        CancellationToken cancellationToken = default);
}
