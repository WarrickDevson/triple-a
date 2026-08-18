namespace KPW.Application.DTOs.SoapNotes;

public record ParseSoapNarrativeRequestDto(
    string Transcript,
    int? PetId = null,
    string? PetName = null,
    string? Species = null,
    string? TargetSection = null);

public record StructuredSoapNoteDto(
    string Subjective,
    string Objective,
    string Action,
    string Plan,
    int? StiffnessScore,
    int? PainScore,
    int? LamenessScore,
    List<CustomMetricDto> CustomMetrics,
    string? SuggestedDiagnosis,
    string RawTranscript,
    double ConfidenceScore,
    IReadOnlyList<string> ExtractedTerms);

public record SoapTranscriptionResultDto(
    string Transcript,
    StructuredSoapNoteDto? StructuredNote,
    long DurationMs,
    bool UsedLocalFallback);

public record VocabularyCategoryDto(
    string Category,
    IReadOnlyList<string> Terms);

public record SoapVocabularyDto(
    IReadOnlyList<string> Terms,
    IReadOnlyList<VocabularyCategoryDto> Categories,
    IReadOnlyDictionary<string, string> AutoCorrections);

public record PolishSoapSectionRequestDto(
    string SectionName,
    string RawText,
    string? PetName = null,
    string? Species = null,
    string? Condition = null);

public record PolishSoapSectionResponseDto(
    string SectionName,
    string PolishedText,
    IReadOnlyList<string> CorrectionsMade,
    string? ClinicalSummary,
    bool UsedCloudAi);

public record AiConfigStatusDto(
    bool IsCloudAiEnabled,
    string Provider,
    string ModelName,
    bool HasApiKey);
