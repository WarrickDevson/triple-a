using KPW.Application.DTOs.Exercises;
using KPW.Domain.Entities;

namespace KPW.Application.Features.Exercises;

internal static class ExerciseMapper
{
    public static ExerciseDto ToDto(Exercise exercise) =>
        new(
            exercise.ExerciseId,
            exercise.Title,
            exercise.ShortDescription,
            exercise.TargetedMuscles,
            exercise.ClinicalPurpose,
            exercise.SafetyNotes,
            exercise.CommonMistakes,
            exercise.VideoUrl,
            exercise.TargetSpecies,
            exercise.ConditionCategory,
            exercise.DifficultyLevel,
            exercise.Steps
                .OrderBy(s => s.StepNumber)
                .Select(s => new ExerciseStepDto(
                    s.ExerciseStepId,
                    s.StepNumber,
                    s.StepInstruction,
                    s.ImageUrl))
                .ToList());
}
