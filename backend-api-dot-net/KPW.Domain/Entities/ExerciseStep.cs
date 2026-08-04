using KPW.Domain.Common;

namespace KPW.Domain.Entities;

public class ExerciseStep : AuditableEntity
{
    public int ExerciseStepId { get; set; }
    public int ExerciseId { get; set; }
    public int StepNumber { get; set; }
    public string StepInstruction { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }

    public Exercise Exercise { get; set; } = null!;
}
