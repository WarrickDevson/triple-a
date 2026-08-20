namespace KPW.Application.DTOs.SoapNotes;

public record CustomMetricDto(
    string Name,
    double Value,
    double MinScale,
    double MaxScale,
    string? UnitOrDescriptor);

public record SoapNoteDto(
    int SoapNoteId,
    int PetId,
    int PhysioId,
    string PhysioName,
    int? AppointmentId,
    DateTime SessionDate,
    string Subjective,
    string Objective,
    string Action,
    string Plan,
    int? StiffnessScore,
    int? PainScore,
    int? LamenessScore,
    IReadOnlyList<CustomMetricDto> CustomMetrics,
    bool IsSharedWithOwner,
    DateTime? SharedAtUtc,
    DateTime CreatedAtUtc,
    string? AudioUrl = null,
    string? RawTranscript = null);

public record CreateSoapNoteRequestDto(
    int? AppointmentId,
    DateTime? SessionDate,
    string Subjective,
    string Objective,
    string Action,
    string Plan,
    int? StiffnessScore,
    int? PainScore,
    int? LamenessScore,
    List<CustomMetricDto>? CustomMetrics,
    bool ShareWithOwner = false,
    string? DiagnosisUpdate = null,
    string? AudioUrl = null,
    string? RawTranscript = null);

public record UpdateSoapNoteRequestDto(
    DateTime? SessionDate,
    string Subjective,
    string Objective,
    string Action,
    string Plan,
    int? StiffnessScore,
    int? PainScore,
    int? LamenessScore,
    List<CustomMetricDto>? CustomMetrics,
    bool ShareWithOwner = false,
    string? AudioUrl = null,
    string? RawTranscript = null);

public record SharedReportDto(
    int SharedReportId,
    int PetId,
    int? SoapNoteId,
    int SharedByPhysioId,
    string SharedByPhysioName,
    string Title,
    string ReportType,
    string? Summary,
    DateTime SharedAtUtc);

public record OwnerSubjectiveNoteDto(
    int OwnerSubjectiveNoteId,
    int PetId,
    int OwnerId,
    string OwnerName,
    DateTime NoteDate,
    string Notes,
    int? PainObserved,
    int? EnergyObserved,
    bool IsReviewed);

public record ShareDocumentRequestDto(
    string Title,
    string ReportType,
    string? Summary,
    int? SoapNoteId = null);

public record ToggleSoapNoteShareRequestDto(
    bool ShareWithOwner);

public record CreateOwnerSubjectiveNoteRequestDto(
    string Notes,
    int? PainObserved,
    int? EnergyObserved);

