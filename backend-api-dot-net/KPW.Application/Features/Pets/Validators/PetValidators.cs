using FluentValidation;
using KPW.Application.DTOs.Pets;
using KPW.Domain.Enums;

namespace KPW.Application.Features.Pets.Validators;

public class CreatePetRequestValidator : AbstractValidator<CreatePetRequestDto>
{
    public CreatePetRequestValidator()
    {
        RuleFor(x => x.PetName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Species).Must(s => PetSpecies.All.Contains(s))
            .WithMessage($"Species must be one of: {string.Join(", ", PetSpecies.All)}");
        RuleFor(x => x.Breed).MaximumLength(100);
        RuleFor(x => x.WeightKg).GreaterThan(0).When(x => x.WeightKg.HasValue);
        RuleFor(x => x.BirthDate).LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .When(x => x.BirthDate.HasValue)
            .WithMessage("Birth date cannot be in the future.");

        RuleFor(x => x.InitialMedicalHistory!.Diagnosis)
            .NotEmpty()
            .MaximumLength(250)
            .When(x => x.InitialMedicalHistory is not null);

        RuleFor(x => x.NewOwner!.Email).EmailAddress().When(x => x.NewOwner is not null);
        RuleFor(x => x.NewOwner!.FirstName).NotEmpty().When(x => x.NewOwner is not null);
        RuleFor(x => x.NewOwner!.LastName).NotEmpty().When(x => x.NewOwner is not null);
        RuleFor(x => x.NewOwner!.TemporaryPassword).MinimumLength(8).When(x => x.NewOwner is not null);
    }
}

public class UpdatePetRequestValidator : AbstractValidator<UpdatePetRequestDto>
{
    public UpdatePetRequestValidator()
    {
        RuleFor(x => x.PetName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Species).Must(s => PetSpecies.All.Contains(s))
            .WithMessage($"Species must be one of: {string.Join(", ", PetSpecies.All)}");
        RuleFor(x => x.Breed).MaximumLength(100);
        RuleFor(x => x.WeightKg).GreaterThan(0).When(x => x.WeightKg.HasValue);
        RuleFor(x => x.BirthDate).LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .When(x => x.BirthDate.HasValue)
            .WithMessage("Birth date cannot be in the future.");
    }
}
