using KPW.Domain.Common;

namespace KPW.Domain.Entities;

public class DailyTrackingLog : AuditableEntity
{
    public int LogId { get; set; }
    public int PetId { get; set; }
    public DateOnly LogDate { get; set; }
    public int? PainScore { get; set; }
    public int? LamenessScore { get; set; }
    public int? EnergyScore { get; set; }
    public int? AppetiteScore { get; set; }
    public int? MobilityScore { get; set; }
    public decimal? WeightKg { get; set; }
    public bool IsCompleted { get; set; }

    public Pet Pet { get; set; } = null!;
}
