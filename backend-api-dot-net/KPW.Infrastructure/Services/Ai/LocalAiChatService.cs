using System.Text.RegularExpressions;
using KPW.Application.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KPW.Infrastructure.Services.Ai;

public partial class LocalAiChatService : IAiChatService
{
    private const string FallbackMessage =
        "I don't have enough approved clinical information to answer that confidently. " +
        "Please book a consultation with your physiotherapist at Triple A Veterinary Physiotherapy for personalised guidance.";

    private readonly IReadOnlyList<EducationChunk> _chunks;
    private readonly ILogger<LocalAiChatService> _logger;

    public LocalAiChatService(IHostEnvironment environment, ILogger<LocalAiChatService> logger)
    {
        _logger = logger;
        _chunks = EducationDocumentLoader.Load(environment);
        _logger.LogInformation("Loaded {Count} education chunks for local AI chat.", _chunks.Count);
    }

    public Task<AiChatResult> ChatAsync(string message, CancellationToken cancellationToken = default)
    {
        var topChunks = EducationChunkRetriever.RetrieveTopChunks(_chunks, message);
        if (topChunks.Count == 0)
        {
            return Task.FromResult(new AiChatResult(FallbackMessage, []));
        }

        var sources = topChunks
            .Select(c => new AiChatSource(c.Title, Truncate(c.Content, 180)))
            .ToList();

        var intro = "Based on Triple A Veterinary Physiotherapy educational materials:";
        var body = string.Join(" ", topChunks.Select(c => c.Content));
        return Task.FromResult(new AiChatResult($"{intro} {body}", sources));
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength].TrimEnd() + "…";
}
