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
        string? audioUrl = null;
        string? rawTranscript = null;

        if (!string.IsNullOrWhiteSpace(note.CustomMetricsJson))
        {
            try
            {
                var trimmed = note.CustomMetricsJson.Trim();
                if (trimmed.StartsWith("["))
                {
                    metrics = JsonSerializer.Deserialize<List<CustomMetricDto>>(note.CustomMetricsJson, JsonOptions) ?? [];
                }
                else if (trimmed.StartsWith("{"))
                {
                    using var doc = JsonDocument.Parse(note.CustomMetricsJson);
                    var root = doc.RootElement;
                    if (root.TryGetProperty("metrics", out var metricsEl) && metricsEl.ValueKind == JsonValueKind.Array)
                    {
                        metrics = JsonSerializer.Deserialize<List<CustomMetricDto>>(metricsEl.GetRawText(), JsonOptions) ?? [];
                    }
                    if (root.TryGetProperty("audioUrl", out var audioEl) && audioEl.ValueKind == JsonValueKind.String)
                    {
                        audioUrl = audioEl.GetString();
                    }
                    if (root.TryGetProperty("rawTranscript", out var rawEl) && rawEl.ValueKind == JsonValueKind.String)
                    {
                        rawTranscript = rawEl.GetString();
                    }
                }
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
            note.CreatedDate,
            audioUrl,
            rawTranscript);
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
