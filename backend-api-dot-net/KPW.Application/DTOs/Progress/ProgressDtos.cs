namespace KPW.Application.DTOs.Progress;

public record PetProgressLogDto(
    DateOnly LogDate,
    int? PainScore,
    int? LamenessScore,
    int? EnergyScore,
    int? AppetiteScore,
    int? MobilityScore,
    decimal? WeightKg,
    bool IsCompleted);

public record PetProgressSummaryDto(
    int PetId,
    string PetName,
    int TotalCompletedSessions,
    int TotalTrackedDays,
    IReadOnlyList<PetProgressLogDto> Logs);
