using KPW.Domain.Common;

namespace KPW.Domain.Entities;

public class RehabProgram : AuditableEntity
{
    public int RehabProgramId { get; set; }
    public int PhysioId { get; set; }
    public int PetId { get; set; }
    public string ProgramTitle { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? Notes { get; set; }

    public User Physio { get; set; } = null!;
    public Pet Pet { get; set; } = null!;
    public ICollection<RehabProgramExercise> RehabProgramExercises { get; set; } = [];
}
