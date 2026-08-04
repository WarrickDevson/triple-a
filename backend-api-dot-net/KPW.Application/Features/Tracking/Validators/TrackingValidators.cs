using FluentValidation;
using KPW.Application.DTOs.Tracking;

namespace KPW.Application.Features.Tracking.Validators;

public class UpsertTrackingRequestValidator : AbstractValidator<UpsertTrackingRequestDto>
{
    public UpsertTrackingRequestValidator()
    {
        RuleFor(x => x)
            .Must(x => x.PainScore.HasValue || x.EnergyScore.HasValue || x.MobilityScore.HasValue ||
                       x.AppetiteScore.HasValue || x.LamenessScore.HasValue || x.WeightKg.HasValue)
            .WithMessage("At least one tracking metric must be provided.");

        RuleFor(x => x.PainScore).InclusiveBetween(1, 10).When(x => x.PainScore.HasValue);
        RuleFor(x => x.EnergyScore).InclusiveBetween(1, 10).When(x => x.EnergyScore.HasValue);
        RuleFor(x => x.MobilityScore).InclusiveBetween(1, 10).When(x => x.MobilityScore.HasValue);
        RuleFor(x => x.AppetiteScore).InclusiveBetween(1, 10).When(x => x.AppetiteScore.HasValue);
        RuleFor(x => x.LamenessScore).InclusiveBetween(1, 10).When(x => x.LamenessScore.HasValue);
        RuleFor(x => x.WeightKg).GreaterThan(0).When(x => x.WeightKg.HasValue);
    }
}
