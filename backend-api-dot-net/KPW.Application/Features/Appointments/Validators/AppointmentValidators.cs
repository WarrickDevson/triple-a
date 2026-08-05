using FluentValidation;
using KPW.Application.DTOs.Appointments;
using KPW.Domain.Enums;

namespace KPW.Application.Features.Appointments.Validators;

public class CreateAppointmentRequestValidator : AbstractValidator<CreateAppointmentRequestDto>
{
    public CreateAppointmentRequestValidator()
    {
        RuleFor(x => x.PetId).GreaterThan(0);
        RuleFor(x => x.ScheduledDateTime)
            .Must(d => d > DateTime.UtcNow)
            .WithMessage("Scheduled date and time must be in the future.");
        RuleFor(x => x.ClientNotes).MaximumLength(500);
        RuleFor(x => x.ClinicianNotes).MaximumLength(4000);
    }
}

public class UpdateAppointmentStatusRequestValidator : AbstractValidator<UpdateAppointmentStatusRequestDto>
{
    private static readonly string[] AllowedStatuses =
    [
        AppointmentStatus.Requested,
        AppointmentStatus.Scheduled,
        AppointmentStatus.Completed,
        AppointmentStatus.Cancelled,
        AppointmentStatus.Rejected
    ];

    public UpdateAppointmentStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(s => AllowedStatuses.Contains(s))
            .WithMessage("Status must be Requested, Scheduled, Completed, Cancelled, or Rejected.");
        RuleFor(x => x.ClinicianNotes).MaximumLength(4000);
    }
}
