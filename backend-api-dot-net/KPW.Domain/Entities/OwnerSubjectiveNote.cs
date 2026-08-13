using KPW.Domain.Common;

namespace KPW.Domain.Entities;

public class OwnerSubjectiveNote : AuditableEntity
{
    public int OwnerSubjectiveNoteId { get; set; }
    public int PetId { get; set; }
    public int OwnerId { get; set; }
    public DateTime NoteDate { get; set; } = DateTime.UtcNow;

    public string Notes { get; set; } = string.Empty;
    public int? PainObserved { get; set; }
    public int? EnergyObserved { get; set; }
    public bool IsReviewed { get; set; }

    public Pet Pet { get; set; } = null!;
    public User Owner { get; set; } = null!;
}
