using KPW.Application.DTOs.SoapNotes;

namespace KPW.Application.Interfaces;

public interface ISoapReportPdfGenerator
{
    byte[] Generate(SoapNoteDto soapNote, string petName, string species, string? breed, string ownerName);
}
