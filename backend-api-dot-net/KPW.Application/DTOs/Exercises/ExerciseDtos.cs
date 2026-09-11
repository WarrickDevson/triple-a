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
    string? CoverImageUrl,
    string? TargetSpecies,
    string? ConditionCategory,
    int DifficultyLevel,
    bool IsSystemDefault,
    int? ClinicId,
    int? BaseExerciseId,
    bool IsActiveForOwners,
    bool HasCustomOverride,
    int? CustomExerciseId,
    bool IsCustomActive,
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
    string? CoverImageUrl,
    string? TargetSpecies,
    string? ConditionCategory,
    int DifficultyLevel,
    IReadOnlyList<CreateExerciseStepRequestDto>? Steps);

public record UpdateExerciseRequestDto(
    string Title,
    string? ShortDescription,
    string? TargetedMuscles,
    string? ClinicalPurpose,
    string? SafetyNotes,
    string? CommonMistakes,
    string? VideoUrl,
    string? CoverImageUrl,
    string? TargetSpecies,
    string? ConditionCategory,
    int DifficultyLevel,
    IReadOnlyList<CreateExerciseStepRequestDto>? Steps);

public record UploadExerciseMediaResultDto(
    string Url,
    string FileName,
    string ContentType,
    bool IsVideo);

