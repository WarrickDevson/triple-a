using KPW.Application.DTOs.Exercises;
using KPW.Application.DTOs.RehabPrograms;
using KPW.Domain.Entities;

namespace KPW.Application.Features.RehabPrograms;

internal static class RehabProgramMapper
{
    public static RehabProgramDto ToDto(RehabProgram program) =>
        new(
            program.RehabProgramId,
            program.PhysioId,
            program.PetId,
            program.ProgramTitle,
            program.StartDate,
            program.EndDate,
            program.Notes,
            program.RehabProgramExercises
                .OrderBy(e => e.RehabProgramExerciseId)
                .Select(e => new RehabProgramExerciseDto(
                    e.RehabProgramExerciseId,
                    e.ExerciseId,
                    e.Exercise.Title,
                    e.Repetitions,
                    e.Sets,
                    e.FrequencyPerDay,
                    e.Exercise.ShortDescription,
                    e.Exercise.SafetyNotes,
                    e.Exercise.CommonMistakes,
                    e.Exercise.VideoUrl,
                    e.Exercise.Steps
                        .OrderBy(s => s.StepNumber)
                        .Select(s => new ExerciseStepDto(
                            s.ExerciseStepId,
                            s.StepNumber,
                            s.StepInstruction,
                            s.ImageUrl))
                        .ToList()))
                .ToList());
}
