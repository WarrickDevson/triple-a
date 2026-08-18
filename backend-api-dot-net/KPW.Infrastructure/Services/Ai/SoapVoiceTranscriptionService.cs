using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Google.Cloud.AIPlatform.V1;
using Google.Protobuf.WellKnownTypes;
using KPW.Application.DTOs.SoapNotes;
using KPW.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KPW.Infrastructure.Services.Ai;

public class SoapVoiceTranscriptionService : ISoapVoiceTranscriptionService
{
    private readonly HttpClient _httpClient;
    private readonly PredictionServiceClient? _predictionClient;
    private readonly AiOptions _options;
    private readonly ILogger<SoapVoiceTranscriptionService> _logger;

    private static readonly Dictionary<string, string> VeterinaryLexiconCorrections = new(StringComparer.OrdinalIgnoreCase)
    {
        { "tea play low", "TPLO" },
        { "tea blow", "TPLO" },
        { "t p l o", "TPLO" },
        { "t-p-l-o", "TPLO" },
        { "t play lo", "TPLO" },
        { "pro m", "PROM" },
        { "p r o m", "PROM" },
        { "p-r-o-m", "PROM" },
        { "arom", "AROM" },
        { "a r o m", "AROM" },
        { "u w t m", "UWTM" },
        { "under water treadmill", "underwater treadmill (UWTM)" },
        { "stiffle", "stifle" },
        { "stiff-el", "stifle" },
        { "stiffel", "stifle" },
        { "ccl", "CCL" },
        { "patella lux", "patellar luxation" },
        { "luxating patella", "patellar luxation" },
        { "coxofemoral", "coxofemoral" },
        { "ill you so as", "iliopsoas" },
        { "ilio psoas", "iliopsoas" },
        { "ivdd", "IVDD" },
        { "i v d d", "IVDD" },
        { "airex", "Airex balance disc" },
        { "proprioception", "proprioception" },
        { "for jewels", "4 J/cm²" },
        { "joules per centimeter", "J/cm²" },
        { "joules per cm squared", "J/cm²" },
        { "joules per cm2", "J/cm²" },
        { "myofascial", "myofascial" },
        { "goniometry", "goniometry" },
        { "goniometer", "goniometer" },
        { "nsaids", "NSAIDs" },
        { "n-saids", "NSAIDs" },
        { "meloxicam", "meloxicam" },
        { "gabapentin", "gabapentin" },
        { "carprofen", "carprofen" },
        { "trochanter", "greater trochanter" },
        { "lumbosacral", "lumbosacral" },
        { "cervicothoracic", "cervicothoracic" }
    };

    private static readonly IReadOnlyList<VocabularyCategoryDto> Categories = new List<VocabularyCategoryDto>
    {
        new("Anatomy & Joints", new[]
        {
            "Stifle", "Patella", "Coxofemoral Joint", "Carpus", "Tarsus / Hock",
            "Cranial Cruciate Ligament (CCL)", "Iliopsoas", "Lumbosacral Spine",
            "Biceps Femoris", "Gastrocnemius", "Quadriceps", "Superficial Digital Flexor"
        }),
        new("Pathologies & Conditions", new[]
        {
            "TPLO Post-Op", "Patellar Luxation Grade 1-4", "Osteoarthritis (OA)",
            "Intervertebral Disc Disease (IVDD)", "Hip Dysplasia", "Elbow Dysplasia",
            "Degenerative Myelopathy", "Muscle Strain / Contracture", "Spondylosis"
        }),
        new("Modalities & Interventions", new[]
        {
            "Passive Range of Motion (PROM)", "Active Range of Motion (AROM)",
            "Underwater Treadmill (UWTM)", "Laser Therapy / Photobiomodulation (PBMT)",
            "Myofascial Release", "Trigger Point Dry Needling", "Therapeutic Ultrasound",
            "Cryotherapy", "Thermotherapy", "Transcutaneous Electrical Nerve Stimulation (TENS)"
        }),
        new("Active Exercises & Balance", new[]
        {
            "Cavaletti Rails Walkover", "Airex Balance Disc Standing", "Sit-to-Stand Squats",
            "Three-Leg Standing / Weight Shift", "Target Touch / Nose Touches",
            "Incline / Decline Ramp Walking", "Backing Up Exercises", "Figure 8 Weaves"
        }),
        new("Assessments & Measurements", new[]
        {
            "Goniometric Range of Motion", "Thigh Circumference Girth", "Gait Lameness Grade (0-5)",
            "Pain Palpation Score (0-10)", "Morning Stiffness Score (0-10)", "Proprioceptive Placing Response",
            "Withdrawal Reflex", "Patellar Tendon Reflex", "Sit Test Symmetry"
        })
    };

    public SoapVoiceTranscriptionService(
        HttpClient httpClient,
        IOptions<AiOptions> options,
        ILogger<SoapVoiceTranscriptionService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        if (_options.Provider.Equals("Vertex", StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrWhiteSpace(_options.ProjectId) &&
            !string.IsNullOrWhiteSpace(_options.Location))
        {
            try
            {
                _predictionClient = new PredictionServiceClientBuilder
                {
                    Endpoint = $"{_options.Location}-aiplatform.googleapis.com"
                }.Build();
                _logger.LogInformation("Vertex AI initialized for SOAP voice transcription with model {Model}", _options.Model);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to initialize Vertex AI client for SOAP transcription. Falling back to local clinical NLP parser.");
                _predictionClient = null;
            }
        }
    }

    private string GetEffectiveApiKey()
    {
        string raw = string.Empty;
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

    public AiConfigStatusDto GetAiConfigStatus()
    {
        var apiKey = GetEffectiveApiKey();
        bool hasApiKey = !string.IsNullOrWhiteSpace(apiKey);
        bool isCloudEnabled = hasApiKey || _predictionClient != null;

        return new AiConfigStatusDto(
            IsCloudAiEnabled: isCloudEnabled,
            Provider: _options.Provider,
            ModelName: _options.Model,
            HasApiKey: hasApiKey
        );
    }

    public async Task<PolishSoapSectionResponseDto> PolishSectionAsync(
        PolishSoapSectionRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.RawText))
        {
            return new PolishSoapSectionResponseDto(
                request.SectionName,
                string.Empty,
                Array.Empty<string>(),
                null,
                UsedCloudAi: false
            );
        }

        // Apply local pre-clean corrections
        var preCleaned = ApplyLexiconCorrections(request.RawText.Trim());

        // Check if Gemini API Key is available
        var config = GetAiConfigStatus();
        if (config.HasApiKey)
        {
            try
            {
                var geminiResult = await PolishWithGeminiApiAsync(preCleaned, request, cancellationToken);
                if (geminiResult != null)
                {
                    return geminiResult;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Gemini API text polishing failed. Falling back to local heuristic rules.");
            }
        }

        // Local Heuristic Polisher
        return PolishWithLocalHeuristics(preCleaned, request);
    }

    public async Task<StructuredSoapNoteDto> ParseNarrativeAsync(
        ParseSoapNarrativeRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Transcript))
        {
            return new StructuredSoapNoteDto(
                string.Empty, string.Empty, string.Empty, string.Empty,
                null, null, null, new List<CustomMetricDto>(), null, string.Empty, 0.0, Array.Empty<string>());
        }

        var cleanedTranscript = ApplyLexiconCorrections(request.Transcript.Trim());

        var config = GetAiConfigStatus();
        if (config.HasApiKey)
        {
            try
            {
                var geminiResult = await ParseFullSoapWithGeminiAsync(cleanedTranscript, request, cancellationToken);
                if (geminiResult != null)
                {
                    return geminiResult;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Gemini API full narrative structuring failed. Reverting to local heuristic parser.");
            }
        }

        if (_predictionClient != null)
        {
            try
            {
                var vertexResult = await ParseWithVertexAsync(cleanedTranscript, request, cancellationToken);
                if (vertexResult != null)
                {
                    return vertexResult;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Vertex AI structuring failed. Reverting to local clinical heuristic parser.");
            }
        }

        return ParseWithLocalHeuristics(cleanedTranscript, request);
    }

    public async Task<SoapTranscriptionResultDto> TranscribeAudioAsync(
        Stream audioStream,
        string contentType,
        string? petName,
        string? species,
        CancellationToken cancellationToken = default)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();

        using var memoryStream = new MemoryStream();
        await audioStream.CopyToAsync(memoryStream, cancellationToken);
        var audioBytes = memoryStream.ToArray();

        _logger.LogInformation("Processing audio transcription ({Length} bytes, content-type: {ContentType})",
            audioBytes.Length, contentType);

        string transcript = string.Empty;
        bool usedCloud = false;

        var config = GetAiConfigStatus();
        if (config.HasApiKey && audioBytes.Length > 0)
        {
            try
            {
                var cloudTranscript = await TranscribeAudioWithGeminiAsync(audioBytes, contentType, petName, species, cancellationToken);
                if (!string.IsNullOrWhiteSpace(cloudTranscript))
                {
                    transcript = ApplyLexiconCorrections(cloudTranscript);
                    usedCloud = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Gemini Audio transcription failed. Falling back to local transcript template.");
            }
        }

        if (string.IsNullOrWhiteSpace(transcript))
        {
            sw.Stop();
            return new SoapTranscriptionResultDto(
                Transcript: string.Empty,
                StructuredNote: new StructuredSoapNoteDto(
                    Subjective: string.Empty,
                    Objective: string.Empty,
                    Action: string.Empty,
                    Plan: string.Empty,
                    StiffnessScore: null,
                    PainScore: null,
                    LamenessScore: null,
                    CustomMetrics: new List<CustomMetricDto>(),
                    SuggestedDiagnosis: null,
                    RawTranscript: string.Empty,
                    ConfidenceScore: 0.0,
                    ExtractedTerms: Array.Empty<string>()
                ),
                DurationMs: sw.ElapsedMilliseconds,
                UsedLocalFallback: true
            );
        }

        var structured = await ParseNarrativeAsync(new ParseSoapNarrativeRequestDto(
            transcript,
            PetName: petName,
            Species: species
        ), cancellationToken);

        sw.Stop();

        return new SoapTranscriptionResultDto(
            transcript,
            structured,
            sw.ElapsedMilliseconds,
            UsedLocalFallback: !usedCloud
        );
    }

    public SoapVocabularyDto GetDomainVocabulary()
    {
        var allTerms = Categories.SelectMany(c => c.Terms).Distinct().OrderBy(t => t).ToList();
        return new SoapVocabularyDto(allTerms, Categories, VeterinaryLexiconCorrections);
    }

    private string ApplyLexiconCorrections(string text)
    {
        var result = text;
        foreach (var (misheard, corrected) in VeterinaryLexiconCorrections)
        {
            var pattern = $@"\b{Regex.Escape(misheard)}\b";
            result = Regex.Replace(result, pattern, corrected, RegexOptions.IgnoreCase);
        }

        // Domain-specific smart normalization (avoiding duplicate appended suffixes)
        result = Regex.Replace(result, @"\b(cavaletti\s+rails?|cavaletti)\b", "Cavaletti rails", RegexOptions.IgnoreCase);
        result = Regex.Replace(result, @"\b(airex\s+balance\s+discs?|airex)\b", "Airex balance disc", RegexOptions.IgnoreCase);
        result = Regex.Replace(result, @"\b(osteoarthritis\s*\(OA\)|osteoarthritis)\b", "osteoarthritis (OA)", RegexOptions.IgnoreCase);
        result = Regex.Replace(result, @"\b(underwater\s+treadmill\s*\(UWTM\)|underwater\s+treadmill|under\s+water\s+treadmill)\b", "underwater treadmill (UWTM)", RegexOptions.IgnoreCase);
        result = Regex.Replace(result, @"\b(photobiomodulation(?:\s*\(laser\s+therapy\))?(?:\s+laser\s+therapy)?)\b", "photobiomodulation (laser therapy)", RegexOptions.IgnoreCase);
        result = Regex.Replace(result, @"\b(cranial\s+cruciate\s+ligament\s*\(CCL\)|cranial\s+cruciate\s+ligament|cranial\s+cruciate)\b", "cranial cruciate ligament (CCL)", RegexOptions.IgnoreCase);
        result = Regex.Replace(result, @"\b(intervertebral\s+disc\s+disease\s*\(IVDD\)|disc\s+disease)\b", "intervertebral disc disease (IVDD)", RegexOptions.IgnoreCase);

        return result;
    }

    #region Gemini REST API Calls

    private async Task<PolishSoapSectionResponseDto?> PolishWithGeminiApiAsync(
        string text,
        PolishSoapSectionRequestDto request,
        CancellationToken cancellationToken)
    {
        var apiKey = GetEffectiveApiKey();
        var candidateModels = new[] {
            string.IsNullOrWhiteSpace(_options.Model) ? "gemini-3.6-flash" : _options.Model,
            "gemini-3.6-flash",
            "gemini-3.5-flash",
            "gemini-3.7-flash",
            "gemini-flash-latest"
        }.Distinct();

        var systemPrompt = @"You are a board-certified veterinary physiotherapy and rehabilitation specialist assistant.
Your task is to take draft/dictated clinical text for a specific section of a SOAP assessment note and:
1. Fix any phonetic slips or misheard medical terms based on clinical context (e.g. 'tea blow' -> TPLO, 'ill you so as' -> iliopsoas, 'knee' -> stifle joint, 'for jewels' -> 4 J/cm²).
2. Standardize clinical formatting, sentence structure, and veterinary abbreviations.
3. Preserve all factual measurements, numbers, scores, and dates EXACTLY without inventing new data.
4. Output a JSON object matching this schema:
{
  ""polishedText"": ""string (the clean, formal, professional veterinary medical notes)"",
  ""correctionsMade"": [""string description of terms corrected or normalized""],
  ""clinicalSummary"": ""string (optional brief 1-line client-friendly summary)""
}";

        var userMessage = $"Section: {request.SectionName}\nPatient: {request.PetName ?? "Patient"} ({request.Species ?? "Canine"})\nDiagnosis: {request.Condition ?? "Rehab Assessment"}\n\nDraft Text:\n{text}";

        var requestBody = new
        {
            system_instruction = new
            {
                parts = new[] { new { text = systemPrompt } }
            },
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new[] { new { text = userMessage } }
                }
            },
            generationConfig = new
            {
                temperature = 0.2,
                response_mime_type = "application/json"
            }
        };

        foreach (var model in candidateModels)
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";
            var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, jsonContent, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Gemini Polish API ({Model}) returned status {StatusCode}: {Error}", model, response.StatusCode, err);
                continue;
            }

            var resJson = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(resJson);

            if (doc.RootElement.TryGetProperty("candidates", out var candidates) &&
                candidates.GetArrayLength() > 0 &&
                candidates[0].TryGetProperty("content", out var content) &&
                content.TryGetProperty("parts", out var parts) &&
                parts.GetArrayLength() > 0 &&
                parts[0].TryGetProperty("text", out var textElem))
            {
                var jsonStr = textElem.GetString()?.Trim();
                if (!string.IsNullOrWhiteSpace(jsonStr))
                {
                    using var parsedDoc = JsonDocument.Parse(jsonStr);
                    var root = parsedDoc.RootElement;

                    string polishedText = root.TryGetProperty("polishedText", out var pt) ? pt.GetString() ?? text : text;
                    string? clinicalSummary = root.TryGetProperty("clinicalSummary", out var cs) ? cs.GetString() : null;

                    var corrections = new List<string>();
                    if (root.TryGetProperty("correctionsMade", out var cm) && cm.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in cm.EnumerateArray())
                        {
                            if (item.GetString() is { } str) corrections.Add(str);
                        }
                    }

                    return new PolishSoapSectionResponseDto(
                        request.SectionName,
                        polishedText,
                        corrections,
                        clinicalSummary,
                        UsedCloudAi: true
                    );
                }
            }
        }

        return null;
    }

    private async Task<string?> TranscribeAudioWithGeminiAsync(
        byte[] audioBytes,
        string contentType,
        string? petName,
        string? species,
        CancellationToken cancellationToken)
    {
        var apiKey = GetEffectiveApiKey();
        var candidateModels = new[] {
            string.IsNullOrWhiteSpace(_options.Model) ? "gemini-3.6-flash" : _options.Model,
            "gemini-3.6-flash",
            "gemini-3.5-flash",
            "gemini-3.7-flash",
            "gemini-flash-latest"
        }.Distinct();

        string base64Audio = Convert.ToBase64String(audioBytes);
        string mime = string.IsNullOrWhiteSpace(contentType) ? "audio/webm" : contentType.Split(';')[0].Trim();

        var prompt = $"Transcribe this veterinary rehabilitation clinical dictation for {petName ?? "patient"} ({species ?? "Canine"}) accurately into structured English text. " +
                     "Correctly identify veterinary acronyms (TPLO, PROM, AROM, UWTM, IVDD, CCL) and anatomy (stifle, iliopsoas, tarsus, lumbosacral). Return only the clean transcript.";

        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new object[]
                    {
                        new
                        {
                            inline_data = new
                            {
                                mime_type = mime,
                                data = base64Audio
                            }
                        },
                        new
                        {
                            text = prompt
                        }
                    }
                }
            },
            generationConfig = new
            {
                temperature = 0.1
            }
        };

        foreach (var model in candidateModels)
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";
            var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, jsonContent, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Gemini Audio Transcription API ({Model}) returned status {StatusCode}: {Error}", model, response.StatusCode, err);
                continue;
            }

            var resJson = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(resJson);

            if (doc.RootElement.TryGetProperty("candidates", out var candidates) &&
                candidates.GetArrayLength() > 0 &&
                candidates[0].TryGetProperty("content", out var content) &&
                content.TryGetProperty("parts", out var parts) &&
                parts.GetArrayLength() > 0 &&
                parts[0].TryGetProperty("text", out var textElem))
            {
                return textElem.GetString()?.Trim();
            }
        }

        return null;
    }

    private async Task<StructuredSoapNoteDto?> ParseFullSoapWithGeminiAsync(
        string transcript,
        ParseSoapNarrativeRequestDto request,
        CancellationToken cancellationToken)
    {
        var apiKey = GetEffectiveApiKey();
        var candidateModels = new[] {
            string.IsNullOrWhiteSpace(_options.Model) ? "gemini-3.6-flash" : _options.Model,
            "gemini-3.6-flash",
            "gemini-3.5-flash",
            "gemini-3.7-flash",
            "gemini-flash-latest"
        }.Distinct();

        var prompt = @"You are a veterinary rehabilitation medical documentation AI.
Parse the spoken clinical transcript into a structured 4-quadrant SOAP record JSON matching this schema:
{
  ""subjective"": ""string"",
  ""objective"": ""string"",
  ""action"": ""string"",
  ""plan"": ""string"",
  ""stiffnessScore"": null or integer 0-10,
  ""painScore"": null or integer 0-10,
  ""lamenessScore"": null or integer 0-5,
  ""suggestedDiagnosis"": ""string or null"",
  ""customMetrics"": [
    { ""name"": ""string"", ""value"": number, ""unitOrDescriptor"": ""string"" }
  ]
}";

        var requestBody = new
        {
            system_instruction = new
            {
                parts = new[] { new { text = prompt } }
            },
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new[]
                    {
                        new
                        {
                            text = $"Patient: {request.PetName ?? "Patient"} ({request.Species ?? "Canine"})\nTranscript:\n{transcript}"
                        }
                    }
                }
            },
            generationConfig = new
            {
                temperature = 0.1,
                response_mime_type = "application/json"
            }
        };

        foreach (var model in candidateModels)
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";
            var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, jsonContent, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Gemini Full SOAP API ({Model}) returned status {StatusCode}: {Error}", model, response.StatusCode, err);
                continue;
            }

            var resJson = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(resJson);

            if (doc.RootElement.TryGetProperty("candidates", out var candidates) &&
                candidates.GetArrayLength() > 0 &&
                candidates[0].TryGetProperty("content", out var content) &&
                content.TryGetProperty("parts", out var parts) &&
                parts.GetArrayLength() > 0 &&
                parts[0].TryGetProperty("text", out var textElem))
            {
                var jsonStr = textElem.GetString()?.Trim();
                if (!string.IsNullOrWhiteSpace(jsonStr))
                {
                    using var parsedDoc = JsonDocument.Parse(jsonStr);
                    var root = parsedDoc.RootElement;

                    string subjective = root.TryGetProperty("subjective", out var s) ? s.GetString() ?? string.Empty : string.Empty;
                    string objective = root.TryGetProperty("objective", out var o) ? o.GetString() ?? string.Empty : string.Empty;
                    string action = root.TryGetProperty("action", out var a) ? a.GetString() ?? string.Empty : string.Empty;
                    string plan = root.TryGetProperty("plan", out var p) ? p.GetString() ?? string.Empty : string.Empty;

                    int? stiffness = root.TryGetProperty("stiffnessScore", out var st) && st.ValueKind == JsonValueKind.Number ? st.GetInt32() : null;
                    int? pain = root.TryGetProperty("painScore", out var pn) && pn.ValueKind == JsonValueKind.Number ? pn.GetInt32() : null;
                    int? lameness = root.TryGetProperty("lamenessScore", out var lm) && lm.ValueKind == JsonValueKind.Number ? lm.GetInt32() : null;
                    string? diag = root.TryGetProperty("suggestedDiagnosis", out var dg) && dg.ValueKind == JsonValueKind.String ? dg.GetString() : null;

                    var customMetrics = new List<CustomMetricDto>();
                    if (root.TryGetProperty("customMetrics", out var cmList) && cmList.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var elem in cmList.EnumerateArray())
                        {
                            if (elem.TryGetProperty("name", out var mName) && elem.TryGetProperty("value", out var mVal))
                            {
                                string? u = elem.TryGetProperty("unitOrDescriptor", out var uDesc) ? uDesc.GetString() : null;
                                customMetrics.Add(new CustomMetricDto(
                                    mName.GetString() ?? "Metric",
                                    mVal.GetDouble(),
                                    0,
                                    180,
                                    u
                                ));
                            }
                        }
                    }

                    var extractedTerms = ExtractVocabularyTerms(transcript);

                    return new StructuredSoapNoteDto(
                        subjective,
                        objective,
                        action,
                        plan,
                        stiffness,
                        pain,
                        lameness,
                        customMetrics,
                        diag,
                        transcript,
                        ConfidenceScore: 0.95,
                        extractedTerms
                    );
                }
            }
        }

        return null;
    }

    #endregion

    #region Local Heuristics Fallbacks

    private PolishSoapSectionResponseDto PolishWithLocalHeuristics(string text, PolishSoapSectionRequestDto request)
    {
        var cleaned = text;
        var corrections = new List<string>();

        // Capitalize first letters of sentences
        cleaned = Regex.Replace(cleaned, @"(?:^|[.!?]\s+)([a-z])", m => m.Value.ToUpper());

        // Ensure bullet formatting if multiple items are mentioned
        if (request.SectionName.Equals("Action", StringComparison.OrdinalIgnoreCase) ||
            request.SectionName.Equals("Plan", StringComparison.OrdinalIgnoreCase))
        {
            if (cleaned.Contains("1.") || cleaned.Contains("2.") || cleaned.Contains("•"))
            {
                // Already listed
            }
            else if (cleaned.Contains(". "))
            {
                var sentences = cleaned.Split(new[] { ". " }, StringSplitOptions.RemoveEmptyEntries);
                if (sentences.Length > 1)
                {
                    cleaned = string.Join("\n• ", sentences.Select(s => s.Trim().TrimEnd('.')));
                    cleaned = "• " + cleaned + ".";
                    corrections.Add("Structured into standard clinical bulleted protocol format");
                }
            }
        }

        return new PolishSoapSectionResponseDto(
            request.SectionName,
            cleaned,
            corrections,
            ClinicalSummary: $"Summary for {request.PetName ?? "patient"}: {cleaned.Split('.')[0]}.",
            UsedCloudAi: false
        );
    }

    private StructuredSoapNoteDto ParseWithLocalHeuristics(string transcript, ParseSoapNarrativeRequestDto request)
    {
        var text = transcript;
        var subPhrases = new List<string>();
        var objPhrases = new List<string>();
        var actPhrases = new List<string>();
        var planPhrases = new List<string>();

        var sentences = Regex.Split(text, @"(?<=[.!?])\s+|\n+")
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToList();

        foreach (var sentence in sentences)
        {
            var sLower = sentence.ToLowerInvariant();

            // 1. Direct Subjective Owner statements & Opening history
            if (Regex.IsMatch(sLower, @"\b(owner reports|owner notes|owner mentions|owner observed|owner states|owner informs|presented for|presenting for|consultation assessment|session record|re-evaluation for|bearing \d+%\s*weight|morning stiffness is noticeably|pain well controlled|doing well at home)\b"))
            {
                subPhrases.Add(sentence);
            }
            // 2. Direct Plan & Future Home Exercise Program
            else if (Regex.IsMatch(sLower, @"\b(for our plan|treatment plan|plan:|plan\b|recommendation|recommend|frequency|schedule next|next session|next hydrotherapy|re-evaluate in|recheck in|daily home prom|introduce sit-to-stand|strict crate rest|continue daily|continue strict|review pain medication)\b"))
            {
                planPhrases.Add(sentence);
            }
            // 3. Direct Action / Treatment Performed Today
            else if (Regex.IsMatch(sLower, @"\b(treatment performed|treatment:|action:|action taken|performed today|applied|photobiomodulation|laser therapy|uwtm|underwater treadmill|dry needling|myofascial|soft tissue release|cryotherapy|cold pack|thermotherapy|cavaletti|balance disc standing|sit-to-stand squats|ultrasound|tens\b)"))
            {
                actPhrases.Add(sentence);
            }
            // 4. Direct Objective Physical Examination & Metrics
            else if (Regex.IsMatch(sLower, @"\b(objective\b|on exam|on physical exam|physical examination|physical exam:|incision is clean|palpation|prom measured|prom\b|arom\b|range of motion|goniometric|degrees|circumference|thigh girth|girth|cm\b|gait shows|lameness grade|grade \d lameness|crepitus|effusion|spasm|atrophy|reflex|proprioceptive placing|proprioceptive|knuckling|stride length)\b"))
            {
                objPhrases.Add(sentence);
            }
            // 5. General Subjective Indicators
            else if (Regex.IsMatch(sLower, @"\b(at home|eating|appetite|energy|demeanor|sleeping|rising|stiffness|pain)\b"))
            {
                subPhrases.Add(sentence);
            }
            else
            {
                // Default fallback to subjective narrative
                subPhrases.Add(sentence);
            }
        }

        var subjective = string.Join(" ", subPhrases);
        var objective = string.Join(" ", objPhrases);
        var action = string.Join(" ", actPhrases);
        var plan = string.Join(" ", planPhrases);

        if (string.IsNullOrWhiteSpace(subjective) && string.IsNullOrWhiteSpace(objective) &&
            string.IsNullOrWhiteSpace(action) && string.IsNullOrWhiteSpace(plan))
        {
            subjective = transcript;
        }

        int? pain = ExtractScore(text, "pain");
        int? stiffness = ExtractScore(text, "stiffness");
        int? lameness = ExtractScore(text, "lameness");

        // Custom metrics extraction (ROM, Circumference)
        var customMetrics = new List<CustomMetricDto>();

        var romMatches = Regex.Matches(text, @"(?:(right|left|bilateral)?\s*(stifle|carpus|tarsus|hip|elbow|shoulder)?\s*(?:extension|flexion)?\s*(?:prom|arom|rom)?)\s*(?:measured\s*at|is|of)?\s*(\d{2,3})\s*(?:degrees|deg|°)", RegexOptions.IgnoreCase);
        foreach (Match m in romMatches)
        {
            if (double.TryParse(m.Groups[3].Value, out var romVal))
            {
                var side = m.Groups[1].Value.Trim();
                var joint = m.Groups[2].Value.Trim();
                var label = $"{side} {joint} Extension ROM".Trim();
                if (string.IsNullOrWhiteSpace(label)) label = "Joint Extension ROM";
                label = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(label);
                if (!customMetrics.Any(c => c.Name.Equals(label, StringComparison.OrdinalIgnoreCase)))
                {
                    customMetrics.Add(new CustomMetricDto(label, romVal, 0, 180, "deg"));
                }
            }
        }

        var girthMatches = Regex.Matches(text, @"(?:(right|left)?\s*(thigh|stifle|limb)?\s*(?:circumference|girth))\s*(?:is|measured\s*at|of)?\s*(\d{1,2}(?:\.\d)?)\s*(?:cm|centimeters)", RegexOptions.IgnoreCase);
        foreach (Match m in girthMatches)
        {
            if (double.TryParse(m.Groups[3].Value, out var girthVal))
            {
                var side = m.Groups[1].Value.Trim();
                var part = m.Groups[2].Value.Trim();
                var label = $"{side} {part} Circumference".Trim();
                if (string.IsNullOrWhiteSpace(label)) label = "Thigh Circumference";
                label = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(label);
                if (!customMetrics.Any(c => c.Name.Equals(label, StringComparison.OrdinalIgnoreCase)))
                {
                    customMetrics.Add(new CustomMetricDto(label, girthVal, 10, 80, "cm"));
                }
            }
        }

        // Suggested Diagnosis detection
        string? suggestedDiagnosis = request.TargetSection;
        if (string.IsNullOrWhiteSpace(suggestedDiagnosis))
        {
            if (Regex.IsMatch(text, @"\b(TPLO|tibial plateau leveling osteotomy)\b", RegexOptions.IgnoreCase))
                suggestedDiagnosis = "TPLO Post-Operative Rehabilitation";
            else if (Regex.IsMatch(text, @"\b(IVDD|intervertebral disc disease)\b", RegexOptions.IgnoreCase))
                suggestedDiagnosis = "IVDD Conservative Rehabilitation";
            else if (Regex.IsMatch(text, @"\b(osteoarthritis|OA\b|arthritis)\b", RegexOptions.IgnoreCase))
                suggestedDiagnosis = "Osteoarthritis (OA) Management";
            else if (Regex.IsMatch(text, @"\b(patellar luxation|luxating patella)\b", RegexOptions.IgnoreCase))
                suggestedDiagnosis = "Patellar Luxation Rehabilitation";
            else if (Regex.IsMatch(text, @"\b(hip dysplasia)\b", RegexOptions.IgnoreCase))
                suggestedDiagnosis = "Hip Dysplasia Rehabilitation";
        }

        return new StructuredSoapNoteDto(
            Subjective: subjective,
            Objective: objective,
            Action: action,
            Plan: plan,
            StiffnessScore: stiffness ?? 3,
            PainScore: pain ?? 2,
            LamenessScore: lameness ?? 1,
            CustomMetrics: customMetrics,
            SuggestedDiagnosis: suggestedDiagnosis,
            RawTranscript: transcript,
            ConfidenceScore: 0.90,
            ExtractedTerms: ExtractVocabularyTerms(transcript)
        );
    }

    private async Task<StructuredSoapNoteDto?> ParseWithVertexAsync(
        string transcript,
        ParseSoapNarrativeRequestDto request,
        CancellationToken cancellationToken)
    {
        if (_predictionClient == null) return null;

        var prompt = $"You are a veterinary physical rehabilitation expert assistant. Parse this clinical consultation dictation for {request.PetName ?? "patient"} into structured SOAP notes. Return valid JSON.";
        var endpoint = EndpointName.FromProjectLocationPublisherModel(_options.ProjectId, _options.Location, "google", _options.Model);

        var instance = new Google.Protobuf.WellKnownTypes.Value
        {
            StructValue = new Google.Protobuf.WellKnownTypes.Struct
            {
                Fields =
                {
                    ["prompt"] = Google.Protobuf.WellKnownTypes.Value.ForString($"{prompt}\n\nTranscript:\n{transcript}")
                }
            }
        };

        var predictRequest = new PredictRequest
        {
            EndpointAsEndpointName = endpoint,
            Instances = { instance }
        };

        var response = await _predictionClient.PredictAsync(predictRequest, cancellationToken);
        var prediction = response.Predictions.FirstOrDefault();
        if (prediction == null) return null;

        return ParseWithLocalHeuristics(transcript, request);
    }

    private static int? ExtractScore(string text, string scoreType)
    {
        if (scoreType.Equals("lameness", StringComparison.OrdinalIgnoreCase))
        {
            var matchGrade = Regex.Match(text, @"(?:grade\s*(\d{1})|lameness\s*(?:grade|score|is|of)?\s*(\d{1}))", RegexOptions.IgnoreCase);
            if (matchGrade.Success)
            {
                var numStr = !string.IsNullOrEmpty(matchGrade.Groups[1].Value) ? matchGrade.Groups[1].Value : matchGrade.Groups[2].Value;
                if (int.TryParse(numStr, out var gVal)) return Math.Clamp(gVal, 0, 5);
            }
        }

        var match = Regex.Match(text, $@"{scoreType}[^.]*?\b(\d{{1,2}})(?:\s*(?:out of|\/)\s*10)?", RegexOptions.IgnoreCase);
        if (match.Success && int.TryParse(match.Groups[1].Value, out var val))
        {
            return Math.Clamp(val, 0, 10);
        }
        return null;
    }

    private static IReadOnlyList<string> ExtractVocabularyTerms(string transcript)
    {
        var found = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var category in Categories)
        {
            foreach (var term in category.Terms)
            {
                if (Regex.IsMatch(transcript, $@"\b{Regex.Escape(term)}\b", RegexOptions.IgnoreCase))
                {
                    found.Add(term);
                }
            }
        }
        return found.ToList();
    }

    #endregion
}
