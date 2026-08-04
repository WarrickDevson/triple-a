namespace KPW.Application.DTOs.Appointments;

public record AppointmentDto(
    int AppointmentId,
    int PhysioId,
    string PhysioName,
    int OwnerId,
    string OwnerName,
    int PetId,
    string PetName,
    DateTime ScheduledDateTime,
    string AppointmentStatus,
    string? ClientNotes,
    string? ClinicianNotes);

public record CreateAppointmentRequestDto(
    int PetId,
    DateTime ScheduledDateTime,
    string? ClientNotes,
    string? ClinicianNotes);

public record UpdateAppointmentStatusRequestDto(
    string Status,
    string? ClinicianNotes);
