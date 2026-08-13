using System.Text.Json;
using KPW.Application.DTOs.SoapNotes;
using KPW.Domain.Entities;

namespace KPW.Application.Features.SoapNotes;

public static class SoapNoteMapper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static SoapNoteDto ToDto(SoapNote note)
    {
        var physioName = note.Physio != null
            ? $"{note.Physio.FirstName} {note.Physio.LastName}".Trim()
            : "Clinician";

        List<CustomMetricDto> metrics = [];
        if (!string.IsNullOrWhiteSpace(note.CustomMetricsJson))
        {
            try
            {
                metrics = JsonSerializer.Deserialize<List<CustomMetricDto>>(note.CustomMetricsJson, JsonOptions) ?? [];
            }
            catch
            {
                metrics = [];
            }
        }

        return new SoapNoteDto(
            note.SoapNoteId,
            note.PetId,
            note.PhysioId,
            physioName,
            note.AppointmentId,
            note.SessionDate,
            note.Subjective,
            note.Objective,
            note.Action,
            note.Plan,
            note.StiffnessScore,
            note.PainScore,
            note.LamenessScore,
            metrics,
            note.IsSharedWithOwner,
            note.SharedAtUtc,
            note.CreatedDate);
    }

    public static SharedReportDto ToSharedReportDto(SharedReport report)
    {
        var physioName = report.SharedByPhysio != null
            ? $"{report.SharedByPhysio.FirstName} {report.SharedByPhysio.LastName}".Trim()
            : "Clinician";

        return new SharedReportDto(
            report.SharedReportId,
            report.PetId,
            report.SoapNoteId,
            report.SharedByPhysioId,
            physioName,
            report.Title,
            report.ReportType,
            report.Summary,
            report.SharedAtUtc);
    }
}
