using KPW.Domain.Common;

namespace KPW.Domain.Entities;

public class Exercise : AuditableEntity
{
    public int ExerciseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string? TargetedMuscles { get; set; }
    public string? ClinicalPurpose { get; set; }
    public string? SafetyNotes { get; set; }
    public string? CommonMistakes { get; set; }
    public string? VideoUrl { get; set; }
    public string? TargetSpecies { get; set; }
    public string? ConditionCategory { get; set; }
    public int DifficultyLevel { get; set; } = 1;
    public string? CoverImageUrl { get; set; }

    public bool IsSystemDefault { get; set; } = true;
    public int? ClinicId { get; set; }
    public int? BaseExerciseId { get; set; }
    public bool IsActiveForOwners { get; set; } = true;

    public Clinic? Clinic { get; set; }
    public Exercise? BaseExercise { get; set; }
    public ICollection<Exercise> CustomOverrides { get; set; } = [];

    public ICollection<ExerciseStep> Steps { get; set; } = [];
    public ICollection<RehabProgramExercise> RehabProgramExercises { get; set; } = [];
    public ICollection<VideoSubmission> VideoSubmissions { get; set; } = [];
}

