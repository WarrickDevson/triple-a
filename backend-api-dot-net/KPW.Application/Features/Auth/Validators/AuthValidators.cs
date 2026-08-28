using System.Text.RegularExpressions;
using FluentValidation;
using KPW.Application.DTOs.Auth;

namespace KPW.Application.Features.Auth.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequestDto>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .MaximumLength(128)
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one digit.")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("Password must contain at least one symbol (e.g. !@#$%^&*).");

        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);

        RuleFor(x => x.PhoneNumber)
            .Must(phone =>
            {
                if (string.IsNullOrWhiteSpace(phone)) return true;
                var clean = Regex.Replace(phone, @"[\s\-\(\)]", "");
                return Regex.IsMatch(clean, @"^(\+27|27|0)[6-8][0-9]{8}$");
            })
            .WithMessage("Please enter a valid South African mobile number (e.g. 082 123 4567 or +27 82 123 4567).")
            .MaximumLength(20);

        RuleFor(x => x.InviteCode).MaximumLength(16);
    }
}

public class LoginRequestValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequestDto>
{
    public ForgotPasswordRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
    }
}

public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequestDto>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .MaximumLength(128)
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one digit.")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("Password must contain at least one symbol (e.g. !@#$%^&*).");
    }
}

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequestDto>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty();
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .MaximumLength(128)
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one digit.")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("Password must contain at least one symbol (e.g. !@#$%^&*).");
    }
}

public class VerifyEmailRequestValidator : AbstractValidator<VerifyEmailRequestDto>
{
    public VerifyEmailRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Token).NotEmpty();
    }
}

public class ResendVerificationEmailRequestValidator : AbstractValidator<ResendVerificationEmailRequestDto>
{
    public ResendVerificationEmailRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
    }
}

public class SendOwnerInviteRequestValidator : AbstractValidator<SendOwnerInviteRequestDto>
{
    public SendOwnerInviteRequestValidator()
    {
        RuleFor(x => x.RecipientEmail).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.OwnerName).MaximumLength(100);
    }
}

public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequestDto>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PhoneNumber)
            .Must(phone =>
            {
                if (string.IsNullOrWhiteSpace(phone)) return true;
                var clean = Regex.Replace(phone, @"[\s\-\(\)]", "");
                return Regex.IsMatch(clean, @"^(\+27|27|0)[6-8][0-9]{8}$");
            })
            .WithMessage("Please enter a valid South African mobile number (e.g. 082 123 4567 or +27 82 123 4567).")
            .MaximumLength(20);
        RuleFor(x => x.ClinicName).MaximumLength(200);
    }
}

public class DataDeletionRequestValidator : AbstractValidator<DataDeletionRequestDto>
{
    public DataDeletionRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.RequestType).MaximumLength(100);
        RuleFor(x => x.Reason).MaximumLength(500);
        RuleFor(x => x.AdditionalNotes).MaximumLength(2000);
    }
}

