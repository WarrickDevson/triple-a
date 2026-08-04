using KPW.Application.DTOs.Reports;
using KPW.Application.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace KPW.Infrastructure.Services.Reports;

public class QuestPetReportPdfGenerator : IPetReportPdfGenerator
{
    private static readonly Color PrimaryDark = Color.FromHex("#0C3C54");
    private static readonly Color PrimaryLight = Color.FromHex("#1E6E8E");
    private static readonly Color NeutralDark = Color.FromHex("#212529");
    private static readonly Color NeutralGrey = Color.FromHex("#E9ECEF");

    static QuestPetReportPdfGenerator()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] Generate(PetClinicalReportDto report)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor(NeutralDark));

                page.Header().Element(c => ComposeHeader(c, report));
                page.Content().Element(c => ComposeContent(c, report));
                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Kruger's Pet Wellness · Confidential clinical report · Page ");
                    text.CurrentPageNumber();
                    text.Span(" of ");
                    text.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }

    private static void ComposeHeader(IContainer container, PetClinicalReportDto report)
    {
        container.Column(column =>
        {
            column.Item().Text("Kruger's Pet Wellness")
                .FontSize(18).Bold().FontColor(PrimaryDark);
            column.Item().Text("Clinical Progress Report")
                .FontSize(14).SemiBold().FontColor(PrimaryLight);
            column.Item().PaddingTop(4).Text($"Generated {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC")
                .FontSize(9).FontColor(Colors.Grey.Darken1);
            column.Item().PaddingTop(12).LineHorizontal(1).LineColor(NeutralGrey);
        });
    }

    private static void ComposeContent(IContainer container, PetClinicalReportDto report)
    {
        container.PaddingTop(16).Column(column =>
        {
            column.Spacing(16);

            column.Item().Element(c => ComposeSection(c, "Patient Summary", inner =>
            {
                inner.Item().Text($"Name: {report.PetName}");
                inner.Item().Text($"Owner: {report.OwnerName}");
                inner.Item().Text($"Species: {report.Species}{(report.Breed is not null ? $" · {report.Breed}" : "")}");
                if (report.WeightKg.HasValue)
                {
                    inner.Item().Text($"Weight: {report.WeightKg:0.##} kg");
                }
                if (report.Diagnosis is not null)
                {
                    inner.Item().PaddingTop(6).Text($"Diagnosis: {report.Diagnosis}");
                }
                if (report.InjuryOrCondition is not null)
                {
                    inner.Item().Text($"Condition: {report.InjuryOrCondition}");
                }
            }));

            column.Item().Element(c => ComposeSection(c, "Rehabilitation Program", inner =>
            {
                if (report.ActiveProgram is null)
                {
                    inner.Item().Text("No active rehabilitation program assigned.");
                    return;
                }

                inner.Item().Text($"{report.ActiveProgram.ProgramTitle} · {report.ActiveProgram.StartDate:yyyy-MM-dd}" +
                    (report.ActiveProgram.EndDate.HasValue ? $" to {report.ActiveProgram.EndDate:yyyy-MM-dd}" : " (ongoing)"));

                if (report.ActiveProgram.Exercises.Count == 0)
                {
                    inner.Item().PaddingTop(6).Text("No exercises in program.");
                    return;
                }

                inner.Item().PaddingTop(8).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.ConstantColumn(50);
                        columns.ConstantColumn(40);
                        columns.ConstantColumn(50);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(NeutralGrey).Padding(4).Text("Exercise").Bold();
                        header.Cell().Background(NeutralGrey).Padding(4).Text("Reps").Bold();
                        header.Cell().Background(NeutralGrey).Padding(4).Text("Sets").Bold();
                        header.Cell().Background(NeutralGrey).Padding(4).Text("/Day").Bold();
                    });

                    foreach (var exercise in report.ActiveProgram.Exercises)
                    {
                        table.Cell().BorderBottom(1).BorderColor(NeutralGrey).Padding(4).Text(exercise.Title);
                        table.Cell().BorderBottom(1).BorderColor(NeutralGrey).Padding(4).Text(exercise.Repetitions.ToString());
                        table.Cell().BorderBottom(1).BorderColor(NeutralGrey).Padding(4).Text(exercise.Sets.ToString());
                        table.Cell().BorderBottom(1).BorderColor(NeutralGrey).Padding(4).Text(exercise.FrequencyPerDay.ToString());
                    }
                });
            }));

            column.Item().Element(c => ComposeSection(c, "Progress Summary", inner =>
            {
                inner.Item().Text($"Completed sessions: {report.TotalCompletedSessions}");
                inner.Item().Text($"Tracked days: {report.TotalTrackedDays}");
                inner.Item().PaddingTop(6).Text(report.NarrativeSummary).Italic();
            }));

            column.Item().Element(c => ComposeSection(c, "Daily Tracking History", inner =>
            {
                if (report.Logs.Count == 0)
                {
                    inner.Item().Text("No daily tracking logs recorded.");
                    return;
                }

                inner.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(70);
                        columns.ConstantColumn(35);
                        columns.ConstantColumn(35);
                        columns.ConstantColumn(35);
                        columns.ConstantColumn(35);
                        columns.ConstantColumn(35);
                        columns.ConstantColumn(45);
                        columns.ConstantColumn(45);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(NeutralGrey).Padding(3).Text("Date").Bold().FontSize(8);
                        header.Cell().Background(NeutralGrey).Padding(3).Text("Pain").Bold().FontSize(8);
                        header.Cell().Background(NeutralGrey).Padding(3).Text("Mob").Bold().FontSize(8);
                        header.Cell().Background(NeutralGrey).Padding(3).Text("Eng").Bold().FontSize(8);
                        header.Cell().Background(NeutralGrey).Padding(3).Text("App").Bold().FontSize(8);
                        header.Cell().Background(NeutralGrey).Padding(3).Text("Lam").Bold().FontSize(8);
                        header.Cell().Background(NeutralGrey).Padding(3).Text("Wt").Bold().FontSize(8);
                        header.Cell().Background(NeutralGrey).Padding(3).Text("Done").Bold().FontSize(8);
                    });

                    foreach (var log in report.Logs)
                    {
                        table.Cell().BorderBottom(1).BorderColor(NeutralGrey).Padding(3).Text(log.LogDate.ToString("yyyy-MM-dd")).FontSize(8);
                        table.Cell().BorderBottom(1).BorderColor(NeutralGrey).Padding(3).Text(FormatScore(log.PainScore)).FontSize(8);
                        table.Cell().BorderBottom(1).BorderColor(NeutralGrey).Padding(3).Text(FormatScore(log.MobilityScore)).FontSize(8);
                        table.Cell().BorderBottom(1).BorderColor(NeutralGrey).Padding(3).Text(FormatScore(log.EnergyScore)).FontSize(8);
                        table.Cell().BorderBottom(1).BorderColor(NeutralGrey).Padding(3).Text(FormatScore(log.AppetiteScore)).FontSize(8);
                        table.Cell().BorderBottom(1).BorderColor(NeutralGrey).Padding(3).Text(FormatScore(log.LamenessScore)).FontSize(8);
                        table.Cell().BorderBottom(1).BorderColor(NeutralGrey).Padding(3).Text(log.WeightKg?.ToString("0.#") ?? "—").FontSize(8);
                        table.Cell().BorderBottom(1).BorderColor(NeutralGrey).Padding(3).Text(log.IsCompleted ? "Yes" : "No").FontSize(8);
                    }
                });
            }));
        });
    }

    private static void ComposeSection(IContainer container, string title, Action<ColumnDescriptor> content)
    {
        container.Column(column =>
        {
            column.Item().Text(title).FontSize(12).Bold().FontColor(PrimaryDark);
            column.Item().PaddingTop(6).Column(content);
        });
    }

    private static string FormatScore(int? score) => score?.ToString() ?? "—";
}
