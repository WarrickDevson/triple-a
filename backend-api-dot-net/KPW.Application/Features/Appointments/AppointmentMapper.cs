using KPW.Application.DTOs.Appointments;
using KPW.Domain.Entities;

namespace KPW.Application.Features.Appointments;

internal static class AppointmentMapper
{
    public static AppointmentDto ToDto(Appointment appointment) =>
        new(
            appointment.AppointmentId,
            appointment.PhysioId,
            $"{appointment.Physio.FirstName} {appointment.Physio.LastName}",
            appointment.OwnerId,
            $"{appointment.Owner.FirstName} {appointment.Owner.LastName}",
            appointment.PetId,
            appointment.Pet.PetName,
            appointment.ScheduledDateTime,
            appointment.AppointmentStatus,
            appointment.ClientNotes,
            appointment.ClinicianNotes);
}
