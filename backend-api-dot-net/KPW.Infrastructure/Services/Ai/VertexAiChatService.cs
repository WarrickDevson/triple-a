using System.Text;
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
        "Please book a consultation with your physiotherapist at Triple A Veterinary Physiotherapy for personalised guidance.";

    private readonly PredictionServiceClient _client;
    private readonly AiOptions _options;
    private readonly IAiPromptConfigService _promptConfigService;
    private readonly IReadOnlyList<EducationChunk> _chunks;
    private readonly ILogger<VertexAiChatService> _logger;

    public VertexAiChatService(
        IOptions<AiOptions> options,
        IAiPromptConfigService promptConfigService,
        IHostEnvironment environment,
        ILogger<VertexAiChatService> logger)
    {
        _options = options.Value;
        _promptConfigService = promptConfigService;
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
            "VertexAiChatService initialized with {Count} education chunks for model {Model} in {Location}",
            _chunks.Count,
            GetEffectiveModel(),
            _options.Location);
    }

    private string GetEffectiveModel()
    {
        var envModel = Environment.GetEnvironmentVariable("AI__MODEL") ??
                       Environment.GetEnvironmentVariable("Ai__Model") ??
                       Environment.GetEnvironmentVariable("AI_MODEL");
        var model = !string.IsNullOrWhiteSpace(envModel) ? envModel.Trim().Trim('"', '\'', ' ') : _options.Model;

        if (string.IsNullOrWhiteSpace(model) || model.Contains("3.5", StringComparison.OrdinalIgnoreCase))
        {
            return "gemini-2.0-flash";
        }

        return model;
    }

    public async Task<AiChatResult> ChatAsync(
        string message,
        string? clinicalContext = null,
        AiChatAttachment? attachment = null,
        IReadOnlyList<AiChatHistoryTurn>? history = null,
        CancellationToken cancellationToken = default)
    {
        var trimmed = message.Trim();
        if (string.IsNullOrWhiteSpace(trimmed) && attachment is null)
        {
            return new AiChatResult(FallbackMessage, []);
        }

        var topChunks = EducationChunkRetriever.RetrieveTopChunks(_chunks, trimmed);
        var sources = topChunks
            .Select(c => new AiChatSource(c.Title, Truncate(c.Content, 180)))
            .ToList();

        try
        {
            var cloudResponse = await ChatWithVertexAsync(trimmed, topChunks, clinicalContext, attachment, history, cancellationToken);
            if (!string.IsNullOrWhiteSpace(cloudResponse))
            {
                return new AiChatResult(cloudResponse, sources);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Vertex AI chat processing failed. Reverting to local clinical knowledge fallback.");
        }

        return FallbackToLocalKnowledge(trimmed, topChunks, sources);
    }

    private async Task<string?> ChatWithVertexAsync(
        string message,
        IReadOnlyList<EducationChunk> chunks,
        string? clinicalContext,
        AiChatAttachment? attachment,
        IReadOnlyList<AiChatHistoryTurn>? history,
        CancellationToken cancellationToken)
    {
        var effectiveModel = GetEffectiveModel();
        var candidateModels = new[]
        {
            effectiveModel,
            "gemini-2.0-flash",
            "gemini-1.5-flash"
        }.Distinct();

        var baseSystemPrompt = await _promptConfigService.GetEffectiveSystemPromptAsync(cancellationToken);
        var systemInstructionBuilder = new StringBuilder(baseSystemPrompt);

        if (!string.IsNullOrWhiteSpace(clinicalContext))
        {
            systemInstructionBuilder.AppendLine();
            systemInstructionBuilder.AppendLine();
            systemInstructionBuilder.AppendLine("=== REGISTERED PETS CLINICAL PROFILES (BACKGROUND CONTEXT) ===");
            systemInstructionBuilder.AppendLine(clinicalContext);
        }

        if (chunks.Count > 0)
        {
            systemInstructionBuilder.AppendLine();
            systemInstructionBuilder.AppendLine();
            systemInstructionBuilder.AppendLine("=== APPROVED REFERENCE CLINICAL MATERIAL FROM TRIPLE A ===");
            systemInstructionBuilder.AppendLine(EducationChunkRetriever.BuildGroundingContext(chunks));
        }

        var userMessageBuilder = new StringBuilder();
        if (attachment != null)
        {
            userMessageBuilder.AppendLine("[Pet owner attached a photo/document for your review]");
        }
        userMessageBuilder.AppendLine(string.IsNullOrWhiteSpace(message) ? "Please review the attached photo/document." : message.Trim());

        foreach (var model in candidateModels)
        {
            try
            {
                var request = new GenerateContentRequest
                {
                    Model = $"projects/{_options.ProjectId}/locations/{_options.Location}/publishers/google/models/{model}",
                    SystemInstruction = new Content
                    {
                        Parts = { new Part { Text = systemInstructionBuilder.ToString() } }
                    }
                };

                // Inject preceding conversation turns from this session
                if (history != null && history.Count > 0)
                {
                    var recentHistory = history.TakeLast(8);
                    foreach (var turn in recentHistory)
                    {
                        var role = string.Equals(turn.Role, "assistant", StringComparison.OrdinalIgnoreCase) ||
                                   string.Equals(turn.Role, "model", StringComparison.OrdinalIgnoreCase)
                            ? "model"
                            : "user";

                        request.Contents.Add(new Content
                        {
                            Role = role,
                            Parts = { new Part { Text = turn.Content } }
                        });
                    }
                }

                // Current turn
                var currentTurn = new Content { Role = "user" };

                // Handle multimodal attachment
                if (attachment != null && !string.IsNullOrWhiteSpace(attachment.Base64Data))
                {
                    var mimeType = !string.IsNullOrWhiteSpace(attachment.MimeType)
                        ? attachment.MimeType
                        : "image/jpeg";

                    currentTurn.Parts.Add(new Part
                    {
                        InlineData = new Blob
                        {
                            MimeType = mimeType,
                            Data = Google.Protobuf.ByteString.FromBase64(attachment.Base64Data)
                        }
                    });
                }

                currentTurn.Parts.Add(new Part { Text = userMessageBuilder.ToString().Trim() });
                request.Contents.Add(currentTurn);

                var response = await _client.GenerateContentAsync(request, cancellationToken: cancellationToken);
                var answer = ExtractAnswer(response);

                if (!string.IsNullOrWhiteSpace(answer))
                {
                    return answer;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Vertex AI call failed for model {Model}. Attempting next candidate...", model);
            }
        }

        return null;
    }

    private static string? ExtractAnswer(GenerateContentResponse response) =>
        response.Candidates
            .FirstOrDefault()?.Content?.Parts
            .FirstOrDefault()?.Text?.Trim();

    private static AiChatResult FallbackToLocalKnowledge(
        string message,
        IReadOnlyList<EducationChunk> topChunks,
        List<AiChatSource> sources)
    {
        if (topChunks.Count == 0)
        {
            return new AiChatResult(FallbackMessage, []);
        }

        var intro = "Based on Triple A Veterinary Physiotherapy educational materials:";
        var body = string.Join(" ", topChunks.Select(c => c.Content));
        return new AiChatResult($"{intro} {body}", sources);
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength].TrimEnd() + "…";
}
