using KPW.Domain.Common;

namespace KPW.Domain.Entities;

public class Appointment : AuditableEntity
{
    public int AppointmentId { get; set; }
    public int PhysioId { get; set; }
    public int OwnerId { get; set; }
    public int PetId { get; set; }
    public DateTime ScheduledDateTime { get; set; }
    public string AppointmentStatus { get; set; } = Enums.AppointmentStatus.Scheduled;
    public string? ClientNotes { get; set; }
    public string? ClinicianNotes { get; set; }

    public User Physio { get; set; } = null!;
    public User Owner { get; set; } = null!;
    public Pet Pet { get; set; } = null!;
}
