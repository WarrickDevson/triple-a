namespace KPW.Application.DTOs.Dashboard;

public record DashboardAppointmentDto(
    int AppointmentId,
    string PetName,
    string OwnerName,
    DateTime ScheduledDateTime,
    string AppointmentStatus);

public record PhysioDashboardDto(
    int PatientCount,
    int PendingVideoReviews,
    int TodaysAppointmentsCount,
    IReadOnlyList<DashboardAppointmentDto> TodaysSchedule);
