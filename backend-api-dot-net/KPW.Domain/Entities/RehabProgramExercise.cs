using KPW.Domain.Common;

namespace KPW.Domain.Entities;

public class RehabProgramExercise : AuditableEntity
{
    public int RehabProgramExerciseId { get; set; }
    public int RehabProgramId { get; set; }
    public int ExerciseId { get; set; }
    public int Repetitions { get; set; } = 10;
    public int Sets { get; set; } = 3;
    public int FrequencyPerDay { get; set; } = 1;

    public RehabProgram RehabProgram { get; set; } = null!;
    public Exercise Exercise { get; set; } = null!;
}
