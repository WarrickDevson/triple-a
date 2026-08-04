using KPW.Domain.Common;

namespace KPW.Domain.Entities;

public class MedicalHistory : AuditableEntity
{
    public int MedicalHistoryId { get; set; }
    public int PetId { get; set; }
    public string Diagnosis { get; set; } = string.Empty;
    public string? InjuryOrCondition { get; set; }
    public DateOnly? SurgeryDate { get; set; }
    public string? ClinicianNotes { get; set; }

    public Pet Pet { get; set; } = null!;
}
