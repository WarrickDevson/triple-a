using FluentValidation;
using KPW.Application.DTOs.Videos;

namespace KPW.Application.Features.Videos.Validators;

public class ReviewVideoRequestValidator : AbstractValidator<ReviewVideoRequestDto>
{
    public ReviewVideoRequestValidator()
    {
        RuleFor(x => x.FeedbackNotes)
            .NotEmpty()
            .MaximumLength(2000);
    }
}
