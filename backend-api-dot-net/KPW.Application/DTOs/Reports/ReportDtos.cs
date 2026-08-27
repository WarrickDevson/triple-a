using KPW.Application.DTOs.Progress;
using KPW.Application.DTOs.SoapNotes;

namespace KPW.Application.DTOs.Reports;

public record RehabProgramExerciseReportDto(
    string Title,
    int Repetitions,
    int Sets,
    int FrequencyPerDay);

public record RehabProgramReportDto(
    string ProgramTitle,
    DateOnly StartDate,
    DateOnly? EndDate,
    IReadOnlyList<RehabProgramExerciseReportDto> Exercises);

public record PetClinicalReportDto(
    int PetId,
    string PetName,
    string OwnerName,
    string Species,
    string? Breed,
    decimal? WeightKg,
    string? Diagnosis,
    string? InjuryOrCondition,
    RehabProgramReportDto? ActiveProgram,
    int TotalCompletedSessions,
    int TotalTrackedDays,
    IReadOnlyList<PetProgressLogDto> Logs,
    string NarrativeSummary,
    string ReportType = "progress",
    string? CustomTitle = null,
    string? CustomSummary = null,
    string? DischargeStatus = null,
    string? MaintenancePlan = null,
    string? VeterinarianNotes = null,
    string? OwnerInstructions = null,
    string? PhysioName = null,
    int? InitialPainScore = null,
    int? FinalPainScore = null,
    int? InitialMobilityScore = null,
    int? FinalMobilityScore = null,
    int? InitialLamenessScore = null,
    int? FinalLamenessScore = null,
    IReadOnlyList<SoapNoteDto>? RecentSoapNotes = null);

public record PetReportFileDto(byte[] Content, string FileName);

public record CreateReportRequestDto(
    int PetId,
    string ReportType,
    string Title,
    string? Summary = null,
    string? Diagnosis = null,
    string? DischargeStatus = null,
    string? MaintenancePlan = null,
    string? VeterinarianNotes = null,
    string? OwnerInstructions = null,
    int? SoapNoteId = null,
    bool ShareWithOwner = true);
