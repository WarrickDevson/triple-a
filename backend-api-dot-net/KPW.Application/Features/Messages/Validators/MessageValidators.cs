using FluentValidation;
using KPW.Application.DTOs.Messages;

namespace KPW.Application.Features.Messages.Validators;

public class SendMessageRequestValidator : AbstractValidator<SendMessageRequestDto>
{
    public SendMessageRequestValidator()
    {
        RuleFor(x => x.Body).NotEmpty().MaximumLength(2000);
    }
}
