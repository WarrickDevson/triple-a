using KPW.Application.DTOs.Progress;

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
    string NarrativeSummary);

public record PetReportFileDto(byte[] Content, string FileName);
