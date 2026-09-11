using KPW.Application.DTOs.Exercises;
using KPW.Domain.Entities;

namespace KPW.Application.Features.Exercises;

internal static class ExerciseMapper
{
    public static ExerciseDto ToDto(
        Exercise exercise,
        bool hasCustomOverride = false,
        int? customExerciseId = null,
        bool isCustomActive = false) =>
        new(
            exercise.ExerciseId,
            exercise.Title,
            exercise.ShortDescription,
            exercise.TargetedMuscles,
            exercise.ClinicalPurpose,
            exercise.SafetyNotes,
            exercise.CommonMistakes,
            exercise.VideoUrl,
            exercise.CoverImageUrl,
            exercise.TargetSpecies,
            exercise.ConditionCategory,
            exercise.DifficultyLevel,
            exercise.IsSystemDefault,
            exercise.ClinicId,
            exercise.BaseExerciseId,
            exercise.IsActiveForOwners,
            hasCustomOverride,
            customExerciseId,
            isCustomActive,
            exercise.Steps
                .OrderBy(s => s.StepNumber)
                .Select(s => new ExerciseStepDto(
                    s.ExerciseStepId,
                    s.StepNumber,
                    s.StepInstruction,
                    s.ImageUrl))
                .ToList());
}
