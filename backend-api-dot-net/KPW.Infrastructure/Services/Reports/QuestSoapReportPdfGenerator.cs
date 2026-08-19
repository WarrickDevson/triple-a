using KPW.Application.DTOs.SoapNotes;
using KPW.Application.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace KPW.Infrastructure.Services.Reports;

public class QuestSoapReportPdfGenerator : ISoapReportPdfGenerator
{
    private static readonly Color PrimaryDark = Color.FromHex("#0C3C54");
    private static readonly Color PrimaryLight = Color.FromHex("#1E6E8E");
    private static readonly Color NeutralDark = Color.FromHex("#212529");
    private static readonly Color NeutralGrey = Color.FromHex("#E9ECEF");
    private static readonly Color AccentSage = Color.FromHex("#6B7A4D");

    static QuestSoapReportPdfGenerator()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] Generate(SoapNoteDto soapNote, string petName, string species, string? breed, string ownerName)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor(NeutralDark));

                page.Header().Element(c => ComposeHeader(c, soapNote, petName, species, breed, ownerName));
                page.Content().Element(c => ComposeContent(c, soapNote));
                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Triple A Veterinary Physiotherapy · Clinical SOAP Report · Confidential · Page ");
                    text.CurrentPageNumber();
                    text.Span(" of ");
                    text.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }

    private static void ComposeHeader(
        IContainer container,
        SoapNoteDto soapNote,
        string petName,
        string species,
        string? breed,
        string ownerName)
    {
        container.Column(column =>
        {
            column.Item().Text("Triple A Veterinary Physiotherapy")
                .FontSize(18).Bold().FontColor(PrimaryDark);
            column.Item().Text("Clinical SOAP Assessment & Session Report")
                .FontSize(14).SemiBold().FontColor(PrimaryLight);
            column.Item().PaddingTop(4).Text($"Session Date: {soapNote.SessionDate:yyyy-MM-dd} · Clinician: {soapNote.PhysioName}")
                .FontSize(9).FontColor(Colors.Grey.Darken1);
            column.Item().PaddingTop(8).Row(row =>
            {
                row.RelativeItem().Text($"Patient: {petName} ({species}{(breed != null ? $" - {breed}" : "")})").Bold();
                row.RelativeItem().AlignRight().Text($"Owner: {ownerName}").Bold();
            });
            column.Item().PaddingTop(8).LineHorizontal(1).LineColor(NeutralGrey);
        });
    }

    private static void ComposeContent(IContainer container, SoapNoteDto soapNote)
    {
        container.PaddingTop(16).Column(column =>
        {
            column.Spacing(14);

            // Scores Summary if any score is provided
            if (soapNote.StiffnessScore.HasValue || soapNote.PainScore.HasValue || soapNote.LamenessScore.HasValue || soapNote.CustomMetrics.Count > 0)
            {
                column.Item().Element(c => ComposeSection(c, "Objective Physical Metrics & Ratings", inner =>
                {
                    inner.Item().Row(row =>
                    {
                        if (soapNote.PainScore.HasValue)
                        {
                            row.RelativeItem().Text($"Pain Score: {soapNote.PainScore}/10").Bold().FontColor(AccentSage);
                        }
                        if (soapNote.StiffnessScore.HasValue)
                        {
                            row.RelativeItem().Text($"Stiffness Score: {soapNote.StiffnessScore}/10").Bold().FontColor(AccentSage);
                        }
                        if (soapNote.LamenessScore.HasValue)
                        {
                            row.RelativeItem().Text($"Lameness Grade: {soapNote.LamenessScore}/5").Bold().FontColor(AccentSage);
                        }
                    });

                    if (soapNote.CustomMetrics.Count > 0)
                    {
                        inner.Item().PaddingTop(6).Column(mCol =>
                        {
                            mCol.Item().Text("Custom Clinical Metrics:").SemiBold();
                            foreach (var metric in soapNote.CustomMetrics)
                            {
                                mCol.Item().Text($"• {metric.Name}: {metric.Value} (Scale: {metric.MinScale} - {metric.MaxScale}{(metric.UnitOrDescriptor != null ? $" {metric.UnitOrDescriptor}" : "")})");
                            }
                        });
                    }
                }));
            }

            // S - Subjective
            column.Item().Element(c => ComposeSection(c, "S — Subjective (Owner History & Observations)", inner =>
            {
                inner.Item().Text(string.IsNullOrWhiteSpace(soapNote.Subjective) ? "No subjective notes recorded." : soapNote.Subjective);
            }));

            // O — Objective
            column.Item().Element(c => ComposeSection(c, "O — Objective (Clinical Findings & Examination)", inner =>
            {
                inner.Item().Text(string.IsNullOrWhiteSpace(soapNote.Objective) ? "No objective examination notes recorded." : soapNote.Objective);
            }));

            // A — Action
            column.Item().Element(c => ComposeSection(c, "A — Action (Session Treatment & Exercises)", inner =>
            {
                inner.Item().Text(string.IsNullOrWhiteSpace(soapNote.Action) ? "No treatment details recorded." : soapNote.Action);
            }));

            // P — Plan
            column.Item().Element(c => ComposeSection(c, "P — Plan (Future Care & Home Program)", inner =>
            {
                inner.Item().Text(string.IsNullOrWhiteSpace(soapNote.Plan) ? "No plan specified." : soapNote.Plan);
            }));
        });
    }

    private static void ComposeSection(IContainer container, string title, Action<ColumnDescriptor> content)
    {
        container.Column(column =>
        {
            column.Item().Text(title).FontSize(11).Bold().FontColor(PrimaryDark);
            column.Item().PaddingTop(4).Column(content);
        });
    }
}
