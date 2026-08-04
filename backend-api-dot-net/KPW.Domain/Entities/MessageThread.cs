using KPW.Domain.Common;

namespace KPW.Domain.Entities;

public class MessageThread : AuditableEntity
{
    public int MessageThreadId { get; set; }
    public int PetId { get; set; }
    public int OwnerId { get; set; }
    public int PhysioId { get; set; }

    public Pet Pet { get; set; } = null!;
    public User Owner { get; set; } = null!;
    public User Physio { get; set; } = null!;
    public ICollection<Message> Messages { get; set; } = [];
}
