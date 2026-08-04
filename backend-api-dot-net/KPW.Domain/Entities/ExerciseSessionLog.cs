using KPW.Domain.Common;

namespace KPW.Domain.Entities;

public class ExerciseSessionLog : AuditableEntity
{
    public int ExerciseSessionLogId { get; set; }
    public int PetId { get; set; }
    public int ExerciseId { get; set; }
    public int RehabProgramId { get; set; }
    public DateTime CompletedAt { get; set; }

    public Pet Pet { get; set; } = null!;
    public Exercise Exercise { get; set; } = null!;
    public RehabProgram RehabProgram { get; set; } = null!;
}
