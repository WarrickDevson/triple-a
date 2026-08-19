using FluentValidation;
using KPW.Application.DTOs.Exercises;

namespace KPW.Application.Features.Exercises.Validators;

public class CreateExerciseRequestValidator : AbstractValidator<CreateExerciseRequestDto>
{
    public CreateExerciseRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200)
            .WithMessage("Exercise title is required (max 200 characters).");

        RuleFor(x => x.ShortDescription).MaximumLength(1000);
        RuleFor(x => x.TargetedMuscles).MaximumLength(500);
        RuleFor(x => x.ClinicalPurpose).MaximumLength(1000);
        RuleFor(x => x.SafetyNotes).MaximumLength(1000);
        RuleFor(x => x.CommonMistakes).MaximumLength(1000);
        RuleFor(x => x.VideoUrl).MaximumLength(500);
        RuleFor(x => x.TargetSpecies).MaximumLength(100);
        RuleFor(x => x.ConditionCategory).MaximumLength(100);
        RuleFor(x => x.DifficultyLevel).InclusiveBetween(1, 5);
    }
}
