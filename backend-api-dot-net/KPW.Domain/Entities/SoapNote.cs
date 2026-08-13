using KPW.Domain.Common;

namespace KPW.Domain.Entities;

public class SoapNote : AuditableEntity
{
    public int SoapNoteId { get; set; }
    public int PetId { get; set; }
    public int PhysioId { get; set; }
    public int? AppointmentId { get; set; }
    public DateTime SessionDate { get; set; } = DateTime.UtcNow;

    public string Subjective { get; set; } = string.Empty;
    public string Objective { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Plan { get; set; } = string.Empty;

    public int? StiffnessScore { get; set; }
    public int? PainScore { get; set; }
    public int? LamenessScore { get; set; }
    public string? CustomMetricsJson { get; set; }

    public bool IsSharedWithOwner { get; set; }
    public DateTime? SharedAtUtc { get; set; }

    public Pet Pet { get; set; } = null!;
    public User Physio { get; set; } = null!;
    public Appointment? Appointment { get; set; }
}
