namespace KPW.Domain.Common;

public abstract class AuditableEntity
{
    public DateTime CreatedDate { get; set; }
    public int? CreatedUserId { get; set; }
    public DateTime ModifiedDate { get; set; }
    public int? ModifiedUserId { get; set; }
    public bool IsActive { get; set; } = true;
}
