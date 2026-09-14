using System.Text.Json;
using KPW.Application.DTOs.Exercises;
using KPW.Domain.Entities;

namespace KPW.Application.Features.Exercises;

internal static class ExerciseMapper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static ExerciseDto ToDto(
        Exercise exercise,
        bool hasCustomOverride = false,
        int? customExerciseId = null,
        bool isCustomActive = false)
    {
        IReadOnlyList<ExerciseVideoVariationDto>? variations = null;
        if (!string.IsNullOrWhiteSpace(exercise.VideoVariationsJson))
        {
            try
            {
                variations = JsonSerializer.Deserialize<List<ExerciseVideoVariationDto>>(exercise.VideoVariationsJson, JsonOptions);
            }
            catch
            {
                variations = null;
            }
        }

        return new(
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
                .ToList(),
            variations);
    }
}

