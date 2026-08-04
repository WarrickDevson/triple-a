using Google.Cloud.AIPlatform.V1;
using KPW.Application.Interfaces;
using KPW.Infrastructure;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KPW.Infrastructure.Services.Ai;

public class VertexAiChatService : IAiChatService
{
    private const string FallbackMessage =
        "I don't have enough approved clinical information to answer that confidently. " +
        "Please book a consultation with your physiotherapist at Kruger's Pet Wellness for personalised guidance.";

    private const string DirectSystemInstruction =
        "You are the wellness assistant for Kruger's Pet Wellness, a veterinary physiotherapy practice. " +
        "Only answer questions about pet rehabilitation, home exercises, pain/mobility/energy tracking, recovery expectations, and safe at-home care after injury or surgery. " +
        "Keep answers concise, practical, and compassionate. " +
        "Do not diagnose new conditions, prescribe medication, or give emergency advice—direct those to a veterinarian. " +
        "For pet-specific treatment plans or worsening symptoms, suggest booking a consultation with their physiotherapist. " +
        "Decline unrelated topics politely.";

    private const string RagSystemInstruction =
        "You are an assistant for Kruger's Pet Wellness. Answer only with information present in the retrieved educational texts provided in the user message. " +
        "If the retrieved texts do not contain enough information to answer, suggest booking a consultation with their physiotherapist. " +
        "Do not invent clinical advice. Keep answers concise and compassionate.";

    private readonly PredictionServiceClient _client;
    private readonly AiOptions _options;
    private readonly IReadOnlyList<EducationChunk> _chunks;
    private readonly ILogger<VertexAiChatService> _logger;

    public VertexAiChatService(
        IOptions<AiOptions> options,
        IHostEnvironment environment,
        ILogger<VertexAiChatService> logger)
    {
        _options = options.Value;
        _logger = logger;
        _chunks = EducationDocumentLoader.Load(environment);

        if (string.IsNullOrWhiteSpace(_options.ProjectId) || string.IsNullOrWhiteSpace(_options.Location))
        {
            throw new InvalidOperationException("Ai:ProjectId and Ai:Location are required when Ai:Provider is Vertex.");
        }

        _client = new PredictionServiceClientBuilder
        {
            Endpoint = $"{_options.Location}-aiplatform.googleapis.com"
        }.Build();

        _logger.LogInformation(
            "Vertex AI chat configured for model {Model} in {Location} (RAG chunks: {UseRag})",
            _options.Model,
            _options.Location,
            _options.UseEducationChunks);
    }

    public async Task<AiChatResult> ChatAsync(string message, CancellationToken cancellationToken = default)
    {
        var trimmed = message.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return new AiChatResult(FallbackMessage, []);
        }

        if (_options.UseEducationChunks)
        {
            return await ChatWithRagAsync(trimmed, cancellationToken);
        }

        return await ChatDirectAsync(trimmed, cancellationToken);
    }

    private async Task<AiChatResult> ChatDirectAsync(string message, CancellationToken cancellationToken)
    {
        var request = BuildRequest(DirectSystemInstruction, message);

        try
        {
            var response = await _client.GenerateContentAsync(request, cancellationToken: cancellationToken);
            var answer = ExtractAnswer(response);

            if (string.IsNullOrWhiteSpace(answer))
            {
                return new AiChatResult(FallbackMessage, []);
            }

            return new AiChatResult(answer, []);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Vertex AI direct chat request failed.");
            return new AiChatResult(FallbackMessage, []);
        }
    }

    private async Task<AiChatResult> ChatWithRagAsync(string message, CancellationToken cancellationToken)
    {
        var topChunks = EducationChunkRetriever.RetrieveTopChunks(_chunks, message);
        if (topChunks.Count == 0)
        {
            return new AiChatResult(FallbackMessage, []);
        }

        var sources = topChunks
            .Select(c => new AiChatSource(c.Title, Truncate(c.Content, 180)))
            .ToList();

        var groundingContext = EducationChunkRetriever.BuildGroundingContext(topChunks);
        var userPrompt = $"Retrieved educational texts:\n{groundingContext}\n\nOwner question: {message}";

        var request = BuildRequest(RagSystemInstruction, userPrompt);

        try
        {
            var response = await _client.GenerateContentAsync(request, cancellationToken: cancellationToken);
            var answer = ExtractAnswer(response);

            if (string.IsNullOrWhiteSpace(answer))
            {
                return new AiChatResult(FallbackMessage, sources);
            }

            return new AiChatResult(answer, sources);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Vertex AI RAG chat request failed.");
            return new AiChatResult(FallbackMessage, sources);
        }
    }

    private GenerateContentRequest BuildRequest(string systemInstruction, string userContent)
    {
        var modelResource =
            $"projects/{_options.ProjectId}/locations/{_options.Location}/publishers/google/models/{_options.Model}";

        return new GenerateContentRequest
        {
            Model = modelResource,
            SystemInstruction = new Content
            {
                Parts = { new Part { Text = systemInstruction } }
            },
            Contents =
            {
                new Content
                {
                    Role = "user",
                    Parts = { new Part { Text = userContent } }
                }
            }
        };
    }

    private static string? ExtractAnswer(GenerateContentResponse response) =>
        response.Candidates
            .FirstOrDefault()?.Content?.Parts
            .FirstOrDefault()?.Text?.Trim();

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength].TrimEnd() + "…";
}
