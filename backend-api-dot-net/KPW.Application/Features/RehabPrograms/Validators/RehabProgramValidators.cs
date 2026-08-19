using FluentValidation;
using KPW.Application.DTOs.RehabPrograms;

namespace KPW.Application.Features.RehabPrograms.Validators;

public class CreateRehabProgramRequestValidator : AbstractValidator<CreateRehabProgramRequestDto>
{
    public CreateRehabProgramRequestValidator()
    {
        RuleFor(x => x.PetId).GreaterThan(0);
        RuleFor(x => x.ProgramTitle).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Exercises).NotNull();
        RuleForEach(x => x.Exercises).ChildRules(exercise =>
        {
            exercise.RuleFor(e => e.ExerciseId).GreaterThan(0);
            exercise.RuleFor(e => e.Repetitions).GreaterThan(0);
            exercise.RuleFor(e => e.Sets).GreaterThan(0);
            exercise.RuleFor(e => e.FrequencyPerDay).GreaterThan(0);
        });
    }
}

public class CompleteExerciseSessionRequestValidator : AbstractValidator<CompleteExerciseSessionRequestDto>
{
    public CompleteExerciseSessionRequestValidator()
    {
        RuleFor(x => x.RehabProgramId).GreaterThan(0);
        RuleFor(x => x.ExerciseId).GreaterThan(0);
    }
}
