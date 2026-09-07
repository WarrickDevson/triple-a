using System.Text;
using System.Text.Json;
using KPW.Application.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KPW.Infrastructure.Services.Ai;

public class GeminiAiChatService : IAiChatService
{
    private const string FallbackMessage =
        "I don't have enough approved clinical information to answer that confidently. " +
        "Please book a consultation with your physiotherapist at Triple A Veterinary Physiotherapy for personalised guidance.";

    private const string SystemInstruction =
        "You are the wellness assistant for Triple A Veterinary Physiotherapy. " +
        "Support pet owners with compassionate, practical, and medically safe guidance regarding pet rehabilitation, recovery expectations, mobility, home exercises, and post-surgery care.\n\n" +
        "Guidelines:\n" +
        "1. Focus directly on the owner's immediate question.\n" +
        "2. NEVER diagnose medical conditions, prescribe medications, or alter dosages. Direct medical concerns to a veterinarian.\n" +
        "3. For worsening pain, severe lameness, or post-surgical red flags, advise prompt evaluation with their vet or Triple A physiotherapist.\n" +
        "4. If asked about appointments or scheduling, guide the owner to the Appointments tab in the Triple A app or clinic reception.\n" +
        "5. Keep responses concise, clear, and structured with bullet points where helpful.\n" +
        "6. When reviewing attached photos or documents, provide observational guidance while noting photos do not replace in-person exams.";

    private readonly HttpClient _httpClient;
    private readonly AiOptions _options;
    private readonly IReadOnlyList<EducationChunk> _chunks;
    private readonly IAiPromptConfigService _promptConfigService;
    private readonly ILogger<GeminiAiChatService> _logger;

    public GeminiAiChatService(
        HttpClient httpClient,
        IOptions<AiOptions> options,
        IHostEnvironment environment,
        IAiPromptConfigService promptConfigService,
        ILogger<GeminiAiChatService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _promptConfigService = promptConfigService;
        _logger = logger;
        _chunks = EducationDocumentLoader.Load(environment);
        _logger.LogInformation("GeminiAiChatService initialized with {Count} education chunks. Configured model: {Model}",
            _chunks.Count, _options.Model);
    }

    private string GetEffectiveApiKey()
    {
        string raw;
        if (!string.IsNullOrWhiteSpace(_options.ApiKey) &&
            !_options.ApiKey.Contains("YOUR_GEMINI_API_KEY", StringComparison.OrdinalIgnoreCase))
        {
            raw = _options.ApiKey;
        }
        else
        {
            raw = Environment.GetEnvironmentVariable("AI__APIKEY") ??
                  Environment.GetEnvironmentVariable("Ai__ApiKey") ??
                  Environment.GetEnvironmentVariable("GEMINI_API_KEY") ??
                  Environment.GetEnvironmentVariable("GOOGLE_API_KEY") ??
                  Environment.GetEnvironmentVariable("AI_API_KEY") ?? string.Empty;
        }

        return raw.Trim().Trim('"', '\'', ' ');
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

        var apiKey = GetEffectiveApiKey();
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            try
            {
                var cloudResponse = await ChatWithGeminiAsync(trimmed, topChunks, clinicalContext, attachment, history, apiKey, cancellationToken);
                if (!string.IsNullOrWhiteSpace(cloudResponse))
                {
                    return new AiChatResult(cloudResponse, sources);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Gemini AI chat call failed. Reverting to local clinical knowledge fallback.");
            }
        }
        else
        {
            _logger.LogWarning("No Google AI / Gemini API key found for wellness assistant. Using local educational chunks.");
        }

        return FallbackToLocalKnowledge(trimmed, topChunks, sources);
    }

    private string GetEffectiveModel()
    {
        var envModel = Environment.GetEnvironmentVariable("AI_MODEL") ??
                       Environment.GetEnvironmentVariable("Ai__Model");
        if (!string.IsNullOrWhiteSpace(envModel))
        {
            return envModel.Trim().Trim('"', '\'', ' ');
        }

        if (!string.IsNullOrWhiteSpace(_options.Model))
        {
            return _options.Model.Trim();
        }

        return "gemini-3.5-flash-lite";
    }

    private async Task<string?> ChatWithGeminiAsync(
        string message,
        IReadOnlyList<EducationChunk> chunks,
        string? clinicalContext,
        AiChatAttachment? attachment,
        IReadOnlyList<AiChatHistoryTurn>? history,
        string apiKey,
        CancellationToken cancellationToken)
    {
        var effectiveModel = GetEffectiveModel();
        var candidateModels = new[] {
            effectiveModel,
            "gemini-3.5-flash-lite",
            "gemini-3.6-flash",
            "gemini-3.5-flash",
            "gemini-3.7-flash",
            "gemini-flash-latest"
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

        var partsList = new List<object>();

        // Handle multimodal attachment
        if (attachment != null)
        {
            var base64Data = attachment.Base64Data;
            var mimeType = !string.IsNullOrWhiteSpace(attachment.MimeType)
                ? attachment.MimeType
                : "image/jpeg";

            if (string.IsNullOrWhiteSpace(base64Data) && !string.IsNullOrWhiteSpace(attachment.Url))
            {
                try
                {
                    var bytes = await _httpClient.GetByteArrayAsync(attachment.Url, cancellationToken);
                    base64Data = Convert.ToBase64String(bytes);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to download attachment from {Url} for Gemini analysis.", attachment.Url);
                }
            }

            if (!string.IsNullOrWhiteSpace(base64Data))
            {
                partsList.Add(new
                {
                    inline_data = new
                    {
                        mime_type = mimeType,
                        data = base64Data
                    }
                });
            }
        }

        partsList.Add(new
        {
            text = userMessageBuilder.ToString()
        });

        var contentsList = new List<object>();

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

                contentsList.Add(new
                {
                    role = role,
                    parts = new object[]
                    {
                        new { text = turn.Content }
                    }
                });
            }
        }

        // Current turn
        contentsList.Add(new
        {
            role = "user",
            parts = partsList.ToArray()
        });

        var requestBody = new
        {
            system_instruction = new
            {
                parts = new[] { new { text = systemInstructionBuilder.ToString() } }
            },
            contents = contentsList.ToArray(),
            generationConfig = new
            {
                temperature = 0.3
            }
        };

        var jsonPayload = JsonSerializer.Serialize(requestBody);

        foreach (var model in candidateModels)
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";
            using var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Gemini AI Chat API ({Model}) returned status {StatusCode}: {Error}",
                    model, response.StatusCode, err);
                continue;
            }

            var resJson = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(resJson);

            if (doc.RootElement.TryGetProperty("candidates", out var candidates) &&
                candidates.GetArrayLength() > 0 &&
                candidates[0].TryGetProperty("content", out var resContent) &&
                resContent.TryGetProperty("parts", out var parts) &&
                parts.GetArrayLength() > 0 &&
                parts[0].TryGetProperty("text", out var textElem))
            {
                var text = textElem.GetString()?.Trim();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    return text;
                }
            }
        }

        return null;
    }

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
