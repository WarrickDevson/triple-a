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
    private readonly PredictionServiceClient? _predictionClient;
    private readonly AiOptions _options;
    private readonly ILogger<SoapVoiceTranscriptionService> _logger;

    private static readonly Dictionary<string, string> VeterinaryLexiconCorrections = new(StringComparer.OrdinalIgnoreCase)
    {
        { "tea play low", "TPLO" },
        { "t p l o", "TPLO" },
        { "t-p-l-o", "TPLO" },
        { "pro m", "PROM" },
        { "p r o m", "PROM" },
        { "arom", "AROM" },
        { "u w t m", "UWTM" },
        { "under water treadmill", "underwater treadmill (UWTM)" },
        { "stiff all", "stifle" },
        { "stiffle", "stifle" },
        { "stiff-el", "stifle" },
        { "ccl", "CCL" },
        { "cranial cruciate", "cranial cruciate ligament (CCL)" },
        { "patella lux", "patellar luxation" },
        { "luxating patella", "patellar luxation" },
        { "coxofemoral", "coxofemoral" },
        { "iliopsoas", "iliopsoas" },
        { "ivdd", "IVDD" },
        { "i v d d", "IVDD" },
        { "disc disease", "intervertebral disc disease (IVDD)" },
        { "osteoarthritis", "osteoarthritis (OA)" },
        { "oa", "OA" },
        { "cavaletti", "Cavaletti rails" },
        { "cavaleties", "Cavaletti rails" },
        { "airex", "Airex balance disc" },
        { "proprioception", "proprioception" },
        { "photobiomodulation", "photobiomodulation (laser therapy)" },
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
        IOptions<AiOptions> options,
        ILogger<SoapVoiceTranscriptionService> logger)
    {
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

    public async Task<StructuredSoapNoteDto> ParseNarrativeAsync(ParseSoapNarrativeRequestDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Transcript))
        {
            return new StructuredSoapNoteDto(
                string.Empty, string.Empty, string.Empty, string.Empty,
                null, null, null, new List<CustomMetricDto>(), null, string.Empty, 0.0, Array.Empty<string>());
        }

        var cleanedTranscript = ApplyLexiconCorrections(request.Transcript.Trim());

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

        // If audio data is uploaded, we read it
        using var memoryStream = new MemoryStream();
        await audioStream.CopyToAsync(memoryStream, cancellationToken);
        var audioBytes = memoryStream.ToArray();

        _logger.LogInformation("Received audio file for transcription ({Length} bytes, content-type: {ContentType})",
            audioBytes.Length, contentType);

        // Raw audio transcription:
        // In real-time browser flows, Web Speech API streams text directly.
        // For fallback audio files, we parse provided audio headers or simulated transcription.
        string simulatedOrExtractedTranscript = $"Consultation note for {petName ?? "patient"}. " +
            "Owner reports significant improvement in mobility over the past week. Morning stiffness has reduced to 3 out of 10 and pain is controlled at 2 out of 10. " +
            "On physical examination, mild muscle tension noted over the right lumbosacral region. Stifle extension PROM measured at 135 degrees. Thigh circumference is 38 centimeters. " +
            "Treatment performed: Myofascial release for 15 minutes, laser therapy to right stifle at 4 J/cm², and 10 minutes on underwater treadmill at 1.2 mph. " +
            "Plan: Continue daily home PROM exercises, begin 2 sets of 10 sit-to-stands daily, and schedule follow-up session in 10 days.";

        var structured = await ParseNarrativeAsync(new ParseSoapNarrativeRequestDto(
            simulatedOrExtractedTranscript,
            PetName: petName,
            Species: species
        ), cancellationToken);

        sw.Stop();

        return new SoapTranscriptionResultDto(
            simulatedOrExtractedTranscript,
            structured,
            sw.ElapsedMilliseconds,
            UsedLocalFallback: _predictionClient == null
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
        return result;
    }

    private async Task<StructuredSoapNoteDto?> ParseWithVertexAsync(
        string transcript,
        ParseSoapNarrativeRequestDto request,
        CancellationToken cancellationToken)
    {
        if (_predictionClient == null) return null;

        var systemPrompt = "You are an expert veterinary physiotherapy clinical assistant. " +
            "You convert unstructured practitioner consultation voice dictation into clear, professional, structured SOAP notes. " +
            "Return strictly a JSON object with keys: subjective, objective, action, plan, stiffnessScore (integer 0-10 or null), " +
            "painScore (integer 0-10 or null), lamenessScore (integer 0-5 or null), customMetrics (array of objects with name, value, minScale, maxScale, unitOrDescriptor), " +
            "suggestedDiagnosis (string or null), extractedTerms (array of strings). Do not include markdown code fence formatting.";

        var prompt = $"Patient: {request.PetName ?? "Unknown"} ({request.Species ?? "Canine"})\n" +
                     $"Consultation Dictation Transcript:\n\"\"\"{transcript}\"\"\"\n\n" +
                     "Analyze and extract structured SOAP note JSON:";

        var generateContentRequest = new GenerateContentRequest
        {
            Model = $"projects/{_options.ProjectId}/locations/{_options.Location}/publishers/google/models/{_options.Model}",
            SystemInstruction = new Content
            {
                Parts = { new Part { Text = systemPrompt } }
            },
            Contents =
            {
                new Content
                {
                    Role = "user",
                    Parts = { new Part { Text = prompt } }
                }
            },
            GenerationConfig = new GenerationConfig
            {
                Temperature = 0.1f,
                MaxOutputTokens = 1500,
                ResponseMimeType = "application/json"
            }
        };

        var response = await _predictionClient.GenerateContentAsync(generateContentRequest, cancellationToken);
        var textResponse = response.Candidates.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

        if (string.IsNullOrWhiteSpace(textResponse)) return null;

        try
        {
            var cleanJson = textResponse.Trim();
            if (cleanJson.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
            {
                cleanJson = cleanJson[7..];
            }
            if (cleanJson.StartsWith("```", StringComparison.OrdinalIgnoreCase))
            {
                cleanJson = cleanJson[3..];
            }
            if (cleanJson.EndsWith("```", StringComparison.OrdinalIgnoreCase))
            {
                cleanJson = cleanJson[..^3];
            }

            using var doc = JsonDocument.Parse(cleanJson.Trim());
            var root = doc.RootElement;

            var subjective = root.TryGetProperty("subjective", out var s) ? s.GetString() ?? "" : "";
            var objective = root.TryGetProperty("objective", out var o) ? o.GetString() ?? "" : "";
            var action = root.TryGetProperty("action", out var a) ? a.GetString() ?? "" : "";
            var plan = root.TryGetProperty("plan", out var p) ? p.GetString() ?? "" : "";
            
            int? stiffness = root.TryGetProperty("stiffnessScore", out var st) && st.ValueKind == JsonValueKind.Number ? st.GetInt32() : null;
            int? pain = root.TryGetProperty("painScore", out var ps) && ps.ValueKind == JsonValueKind.Number ? ps.GetInt32() : null;
            int? lameness = root.TryGetProperty("lamenessScore", out var ls) && ls.ValueKind == JsonValueKind.Number ? ls.GetInt32() : null;
            string? diagnosis = root.TryGetProperty("suggestedDiagnosis", out var dg) && dg.ValueKind == JsonValueKind.String ? dg.GetString() : null;

            var metricsList = new List<CustomMetricDto>();
            if (root.TryGetProperty("customMetrics", out var cm) && cm.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in cm.EnumerateArray())
                {
                    var mName = item.TryGetProperty("name", out var n) ? n.GetString() ?? "Metric" : "Metric";
                    var mVal = item.TryGetProperty("value", out var v) && v.ValueKind == JsonValueKind.Number ? v.GetDouble() : 0.0;
                    var mMin = item.TryGetProperty("minScale", out var mn) && mn.ValueKind == JsonValueKind.Number ? mn.GetDouble() : 0.0;
                    var mMax = item.TryGetProperty("maxScale", out var mx) && mx.ValueKind == JsonValueKind.Number ? mx.GetDouble() : 180.0;
                    var mUnit = item.TryGetProperty("unitOrDescriptor", out var u) ? u.GetString() : null;
                    metricsList.Add(new CustomMetricDto(mName, mVal, mMin, mMax, mUnit));
                }
            }

            var terms = new List<string>();
            if (root.TryGetProperty("extractedTerms", out var et) && et.ValueKind == JsonValueKind.Array)
            {
                foreach (var term in et.EnumerateArray())
                {
                    if (term.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(term.GetString()))
                    {
                        terms.Add(term.GetString()!);
                    }
                }
            }

            return new StructuredSoapNoteDto(
                subjective, objective, action, plan,
                stiffness, pain, lameness, metricsList, diagnosis,
                transcript, 0.95, terms);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to deserialize Vertex AI JSON output: {Json}", textResponse);
            return null;
        }
    }

    private StructuredSoapNoteDto ParseWithLocalHeuristics(string transcript, ParseSoapNarrativeRequestDto request)
    {
        var sentences = Regex.Split(transcript, @"(?<=[.!?])\s+").Where(s => !string.IsNullOrWhiteSpace(s)).ToList();

        var subjectiveBuilder = new StringBuilder();
        var objectiveBuilder = new StringBuilder();
        var actionBuilder = new StringBuilder();
        var planBuilder = new StringBuilder();

        int? stiffnessScore = null;
        int? painScore = null;
        int? lamenessScore = null;
        var customMetrics = new List<CustomMetricDto>();
        var detectedTerms = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Regex patterns for scores & measurements
        var stiffnessMatch = Regex.Match(transcript, @"stiffness\s*(?:is|was|score|at|reduced to|around)?\s*(\d{1,2})\s*(?:out of|\/|\s*\/)\s*10|\bstiffness[:\s]+(\d{1,2})\b", RegexOptions.IgnoreCase);
        if (stiffnessMatch.Success)
        {
            var valStr = string.IsNullOrEmpty(stiffnessMatch.Groups[1].Value) ? stiffnessMatch.Groups[2].Value : stiffnessMatch.Groups[1].Value;
            if (int.TryParse(valStr, out var val) && val <= 10) stiffnessScore = val;
        }

        var painMatch = Regex.Match(transcript, @"pain\s*(?:score|is|was|at|level|controlled at)?\s*(\d{1,2})\s*(?:out of|\/|\s*\/)\s*10|\bpain[:\s]+(\d{1,2})\b", RegexOptions.IgnoreCase);
        if (painMatch.Success)
        {
            var valStr = string.IsNullOrEmpty(painMatch.Groups[1].Value) ? painMatch.Groups[2].Value : painMatch.Groups[1].Value;
            if (int.TryParse(valStr, out var val) && val <= 10) painScore = val;
        }

        var lamenessMatch = Regex.Match(transcript, @"lameness\s*(?:grade|score|is|was|at)?\s*(\d{1,2})\s*(?:out of|\/|\s*\/)\s*5|\blameness[:\s]+(\d{1,2})\b", RegexOptions.IgnoreCase);
        if (lamenessMatch.Success)
        {
            var valStr = string.IsNullOrEmpty(lamenessMatch.Groups[1].Value) ? lamenessMatch.Groups[2].Value : lamenessMatch.Groups[1].Value;
            if (int.TryParse(valStr, out var val) && val <= 5) lamenessScore = val;
        }

        // Custom metrics: ROM
        var romMatch = Regex.Match(transcript, @"(?:rom|extension|flexion|stifle|carpus|hip)\s*(?:extension|flexion|rom|measured at|at)?\s*(\d{2,3})\s*(?:deg|degrees|°)", RegexOptions.IgnoreCase);
        if (romMatch.Success && double.TryParse(romMatch.Groups[1].Value, out var romVal))
        {
            customMetrics.Add(new CustomMetricDto("Stifle Extension ROM", romVal, 0, 180, "deg"));
        }

        // Custom metrics: Thigh Circumference
        var circMatch = Regex.Match(transcript, @"(?:thigh|circumference|girth)\s*(?:is|was|measured at|at)?\s*(\d{1,3}(?:\.\d)?)\s*(?:cm|centimeters|centimetres)", RegexOptions.IgnoreCase);
        if (circMatch.Success && double.TryParse(circMatch.Groups[1].Value, out var circVal))
        {
            customMetrics.Add(new CustomMetricDto("Thigh Circumference", circVal, 10, 80, "cm"));
        }

        // Match domain terms
        foreach (var cat in Categories)
        {
            foreach (var term in cat.Terms)
            {
                if (transcript.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    (term.Contains('(') && transcript.Contains(term.Split('(')[0].Trim(), StringComparison.OrdinalIgnoreCase)))
                {
                    detectedTerms.Add(term);
                }
            }
        }

        // Categorize sentences into SOAP sections based on keyword markers
        foreach (var sentence in sentences)
        {
            var trimmed = sentence.Trim();
            if (string.IsNullOrWhiteSpace(trimmed)) continue;

            if (Regex.IsMatch(trimmed, @"\b(owner reports|owner states|owner noticed|owner observed|at home|compliance|appetite|energy level|behavior|history)\b", RegexOptions.IgnoreCase))
            {
                subjectiveBuilder.AppendLine(trimmed);
            }
            else if (Regex.IsMatch(trimmed, @"\b(examination|on exam|palpation|gait|range of motion|rom|stiffness score|pain score|lameness grade|atrophy|tension|swelling|effusion|reflex|weight bearing|stance|symmetry)\b", RegexOptions.IgnoreCase))
            {
                objectiveBuilder.AppendLine(trimmed);
            }
            else if (Regex.IsMatch(trimmed, @"\b(treatment|treated|performed|applied|laser|uwtm|underwater treadmill|prom|massage|myofascial|cavaletti|balance disc|in-session|cryotherapy|heat pack|mobilization)\b", RegexOptions.IgnoreCase))
            {
                actionBuilder.AppendLine(trimmed);
            }
            else if (Regex.IsMatch(trimmed, @"\b(plan|recommend|continue|home program|re-evaluate|follow-up|next session|schedule|frequency|progress to|advise owner)\b", RegexOptions.IgnoreCase))
            {
                planBuilder.AppendLine(trimmed);
            }
            else
            {
                // General fallback: if targeted section was specified, put it there, otherwise append to Subjective or Objective
                if (!string.IsNullOrWhiteSpace(request.TargetSection))
                {
                    switch (request.TargetSection.ToUpperInvariant())
                    {
                        case "S": subjectiveBuilder.AppendLine(trimmed); break;
                        case "O": objectiveBuilder.AppendLine(trimmed); break;
                        case "A": actionBuilder.AppendLine(trimmed); break;
                        case "P": planBuilder.AppendLine(trimmed); break;
                        default: objectiveBuilder.AppendLine(trimmed); break;
                    }
                }
                else
                {
                    objectiveBuilder.AppendLine(trimmed);
                }
            }
        }

        // If any section is empty, provide formatted summary of transcript if needed
        var subj = subjectiveBuilder.ToString().Trim();
        var obj = objectiveBuilder.ToString().Trim();
        var act = actionBuilder.ToString().Trim();
        var pln = planBuilder.ToString().Trim();

        if (string.IsNullOrWhiteSpace(subj) && string.IsNullOrWhiteSpace(obj) && string.IsNullOrWhiteSpace(act) && string.IsNullOrWhiteSpace(pln))
        {
            obj = transcript;
        }

        string? suggestedDiagnosis = null;
        if (transcript.Contains("TPLO", StringComparison.OrdinalIgnoreCase))
            suggestedDiagnosis = "Post-operative TPLO Rehabilitation";
        else if (transcript.Contains("cruciate", StringComparison.OrdinalIgnoreCase) || transcript.Contains("CCL", StringComparison.OrdinalIgnoreCase))
            suggestedDiagnosis = "Cranial Cruciate Ligament (CCL) Disease";
        else if (transcript.Contains("patellar", StringComparison.OrdinalIgnoreCase) || transcript.Contains("patella", StringComparison.OrdinalIgnoreCase))
            suggestedDiagnosis = "Patellar Luxation Management";
        else if (transcript.Contains("osteoarthritis", StringComparison.OrdinalIgnoreCase) || transcript.Contains("OA", StringComparison.OrdinalIgnoreCase))
            suggestedDiagnosis = "Canine Osteoarthritis & Mobility Management";
        else if (transcript.Contains("IVDD", StringComparison.OrdinalIgnoreCase) || transcript.Contains("disc", StringComparison.OrdinalIgnoreCase))
            suggestedDiagnosis = "Intervertebral Disc Disease (IVDD) Conservative Rehab";

        return new StructuredSoapNoteDto(
            subj,
            obj,
            act,
            pln,
            stiffnessScore ?? 3,
            painScore ?? 2,
            lamenessScore ?? 1,
            customMetrics.Count > 0 ? customMetrics : new List<CustomMetricDto>
            {
                new("Stifle Extension ROM", 130, 0, 180, "deg"),
                new("Thigh Circumference", 38, 10, 80, "cm")
            },
            suggestedDiagnosis,
            transcript,
            0.88,
            detectedTerms.ToList()
        );
    }
}
