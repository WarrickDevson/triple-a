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

    public ICollection<ExerciseStep> Steps { get; set; } = [];
    public ICollection<RehabProgramExercise> RehabProgramExercises { get; set; } = [];
    public ICollection<VideoSubmission> VideoSubmissions { get; set; } = [];
}
