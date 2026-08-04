namespace KPW.Application.DTOs.Tracking;

public record UpsertTrackingRequestDto(
    int? PainScore,
    int? EnergyScore,
    int? MobilityScore,
    int? AppetiteScore,
    int? LamenessScore,
    decimal? WeightKg);

public record DailyTrackingLogDto(
    int LogId,
    int PetId,
    DateOnly LogDate,
    int? PainScore,
    int? LamenessScore,
    int? EnergyScore,
    int? AppetiteScore,
    int? MobilityScore,
    decimal? WeightKg,
    bool IsCompleted);
