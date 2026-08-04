using KPW.Application.DTOs.Exercises;

namespace KPW.Application.DTOs.RehabPrograms;

public record RehabProgramExerciseDto(
    int RehabProgramExerciseId,
    int ExerciseId,
    string Title,
    int Repetitions,
    int Sets,
    int FrequencyPerDay,
    string? ShortDescription,
    string? SafetyNotes,
    string? CommonMistakes,
    string? VideoUrl,
    IReadOnlyList<ExerciseStepDto> Steps);

public record RehabProgramDto(
    int RehabProgramId,
    int PhysioId,
    int PetId,
    string ProgramTitle,
    DateOnly StartDate,
    DateOnly? EndDate,
    string? Notes,
    IReadOnlyList<RehabProgramExerciseDto> Exercises);

public record CreateRehabProgramExerciseDto(
    int ExerciseId,
    int Repetitions,
    int Sets,
    int FrequencyPerDay);

public record CreateRehabProgramRequestDto(
    int PetId,
    string ProgramTitle,
    DateOnly StartDate,
    DateOnly? EndDate,
    string? Notes,
    IReadOnlyList<CreateRehabProgramExerciseDto> Exercises);

public record CompleteExerciseSessionRequestDto(
    int RehabProgramId,
    int ExerciseId);
