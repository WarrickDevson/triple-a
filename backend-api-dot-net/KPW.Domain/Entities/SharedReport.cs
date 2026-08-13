using KPW.Domain.Common;

namespace KPW.Domain.Entities;

public class SharedReport : AuditableEntity
{
    public int SharedReportId { get; set; }
    public int PetId { get; set; }
    public int? SoapNoteId { get; set; }
    public int SharedByPhysioId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string ReportType { get; set; } = "SOAP_SESSION";
    public string? Summary { get; set; }
    public DateTime SharedAtUtc { get; set; } = DateTime.UtcNow;

    public Pet Pet { get; set; } = null!;
    public SoapNote? SoapNote { get; set; }
    public User SharedByPhysio { get; set; } = null!;
}
