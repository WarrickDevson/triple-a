namespace KPW.Application.DTOs.Exercises;

public record ExerciseStepDto(
    int ExerciseStepId,
    int StepNumber,
    string StepInstruction,
    string? ImageUrl);

public record ExerciseDto(
    int ExerciseId,
    string Title,
    string? ShortDescription,
    string? TargetedMuscles,
    string? ClinicalPurpose,
    string? SafetyNotes,
    string? CommonMistakes,
    string? VideoUrl,
    string? TargetSpecies,
    string? ConditionCategory,
    int DifficultyLevel,
    IReadOnlyList<ExerciseStepDto> Steps);

public record CreateExerciseStepRequestDto(
    int StepNumber,
    string StepInstruction,
    string? ImageUrl);

public record CreateExerciseRequestDto(
    string Title,
    string? ShortDescription,
    string? TargetedMuscles,
    string? ClinicalPurpose,
    string? SafetyNotes,
    string? CommonMistakes,
    string? VideoUrl,
    string? TargetSpecies,
    string? ConditionCategory,
    int DifficultyLevel,
    IReadOnlyList<CreateExerciseStepRequestDto>? Steps);
