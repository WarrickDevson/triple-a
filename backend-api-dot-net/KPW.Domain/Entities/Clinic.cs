using KPW.Domain.Common;

namespace KPW.Domain.Entities;

public class Clinic : AuditableEntity
{
    public int ClinicId { get; set; }
    public string ClinicName { get; set; } = string.Empty;
    public string? VatNumber { get; set; }
    public string PhysicalAddress { get; set; } = string.Empty;
    public string ContactNumber { get; set; } = string.Empty;
    public string InviteCode { get; set; } = string.Empty;

    public ICollection<User> Users { get; set; } = [];
}
