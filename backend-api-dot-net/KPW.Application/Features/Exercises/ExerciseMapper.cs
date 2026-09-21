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

        var resolvedCover = ResolveMediaUrl(exercise.CoverImageUrl);
        var resolvedVideo = ResolveMediaUrl(exercise.VideoUrl);

        var resolvedVariations = variations?.Select(v => v with { VideoUrl = ResolveMediaUrl(v.VideoUrl) ?? v.VideoUrl }).ToList();

        return new(
            exercise.ExerciseId,
            exercise.Title,
            exercise.ShortDescription,
            exercise.TargetedMuscles,
            exercise.ClinicalPurpose,
            exercise.SafetyNotes,
            exercise.CommonMistakes,
            resolvedVideo,
            resolvedCover,
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
                    ResolveMediaUrl(s.ImageUrl)))
                .ToList(),
            resolvedVariations);
    }

    private static string? ResolveMediaUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;

        var trimmed = url.Trim();
        if (trimmed.StartsWith("/api/media", StringComparison.OrdinalIgnoreCase))
        {
            return trimmed;
        }

        // If external URL that doesn't point to GCS, return as-is
        if ((trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
             trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) &&
            !trimmed.Contains("storage.googleapis.com", StringComparison.OrdinalIgnoreCase))
        {
            return trimmed;
        }

        // Extract object path from GCS URL if needed
        var path = trimmed;
        if (trimmed.Contains("storage.googleapis.com", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var uri = new Uri(trimmed);
                var absPath = uri.AbsolutePath.TrimStart('/');
                var slashIndex = absPath.IndexOf('/');
                path = slashIndex >= 0 ? absPath[(slashIndex + 1)..] : absPath;
            }
            catch
            {
                // keep path
            }
        }

        var normalized = path.TrimStart('/', '\\');
        return $"/api/media/view?path={Uri.EscapeDataString(normalized)}";
    }
}

