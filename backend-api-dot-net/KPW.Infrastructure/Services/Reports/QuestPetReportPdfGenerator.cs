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
    private static readonly Color AccentSage = Color.FromHex("#4A7C59");
    private static readonly Color AccentSageLight = Color.FromHex("#E8F0EA");
    private static readonly Color NeutralDark = Color.FromHex("#212529");
    private static readonly Color NeutralGrey = Color.FromHex("#E9ECEF");
    private static readonly Color NeutralMuted = Color.FromHex("#6C757D");

    static QuestPetReportPdfGenerator()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] Generate(PetClinicalReportDto report)
    {
        var type = (report.ReportType ?? "progress").ToLowerInvariant();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(36);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor(NeutralDark));

                page.Header().Element(c => ComposeHeader(c, report, type));

                page.Content().Element(c =>
                {
                    if (type.Contains("discharge"))
                    {
                        ComposeDischargeContent(c, report);
                    }
                    else if (type.Contains("home"))
                    {
                        ComposeHomeProgramContent(c, report);
                    }
                    else
                    {
                        ComposeProgressContent(c, report);
                    }
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Triple A Veterinary Physiotherapy · Confidential Clinical Record · Page ");
                    text.CurrentPageNumber();
                    text.Span(" of ");
                    text.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }

    // ================= HEADER =================
    private static void ComposeHeader(IContainer container, PetClinicalReportDto report, string type)
    {
        var title = !string.IsNullOrWhiteSpace(report.CustomTitle)
            ? report.CustomTitle
            : type switch
            {
                var t when t.Contains("discharge") => "Rehabilitation Discharge Summary & End-of-Care Report",
                var t when t.Contains("home") => "Home Exercise Program & Rehabilitation Guide",
                _ => "Clinical Progress & Rehabilitation Report"
            };

        var subtitle = type switch
        {
            var t when t.Contains("discharge") => "Formal Veterinary Physiotherapy Discharge & Outcome Documentation",
            var t when t.Contains("home") => "Personalized Home Care & Guided Exercise Protocol for Pet Owners",
            _ => "Veterinary Physiotherapy Clinical Progress & Outcome Tracking"
        };

        container.Column(column =>
        {
            column.Item().Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("Triple A Veterinary Physiotherapy")
                        .FontSize(16).Bold().FontColor(PrimaryDark);
                    col.Item().Text(title)
                        .FontSize(13).SemiBold().FontColor(PrimaryLight);
                    col.Item().Text(subtitle)
                        .FontSize(8.5f).FontColor(NeutralMuted);
                });

                row.ConstantItem(150).AlignRight().Column(col =>
                {
                    col.Item().Text($"Date: {DateTime.UtcNow:yyyy-MM-dd}").FontSize(8.5f).FontColor(NeutralMuted);
                    col.Item().Text($"Patient: {report.PetName}").FontSize(9).Bold().FontColor(PrimaryDark);
                    col.Item().Text($"ID: #{report.PetId}").FontSize(8).FontColor(NeutralMuted);
                    if (report.PeriodFrom.HasValue || report.PeriodTo.HasValue)
                    {
                        var periodText = (report.PeriodFrom.HasValue && report.PeriodTo.HasValue)
                            ? $"Period: {report.PeriodFrom.Value:dd MMM yyyy} – {report.PeriodTo.Value:dd MMM yyyy}"
                            : report.PeriodFrom.HasValue
                                ? $"Period from: {report.PeriodFrom.Value:dd MMM yyyy}"
                                : $"Period through: {report.PeriodTo!.Value:dd MMM yyyy}";

                        col.Item().PaddingTop(2).Text(periodText).FontSize(7.5f).Bold().FontColor(AccentSage);
                    }
                });
            });

            column.Item().PaddingTop(8).LineHorizontal(1).LineColor(NeutralGrey);
        });
    }

    // ================= PROGRESS REPORT LAYOUT =================
    private static void ComposeProgressContent(IContainer container, PetClinicalReportDto report)
    {
        container.PaddingTop(12).Column(column =>
        {
            column.Spacing(12);

            // Patient Summary
            column.Item().Element(c => ComposeSection(c, "Patient & Clinical Summary", inner =>
            {
                inner.Item().Row(row =>
                {
                    row.RelativeItem().Column(c1 =>
                    {
                        c1.Item().Text($"Patient: {report.PetName}").Bold();
                        c1.Item().Text($"Species: {report.Species}{(report.Breed is not null ? $" · {report.Breed}" : "")}");
                        if (report.WeightKg.HasValue) c1.Item().Text($"Weight: {report.WeightKg:0.##} kg");
                    });
                    row.RelativeItem().Column(c2 =>
                    {
                        c2.Item().Text($"Owner: {report.OwnerName}").Bold();
                        c2.Item().Text($"Diagnosis: {report.Diagnosis ?? "Clinical rehabilitation"}");
                        if (report.InjuryOrCondition != null) c2.Item().Text($"Condition: {report.InjuryOrCondition}");
                    });
                });
            }));

            // Active Rehabilitation Program
            column.Item().Element(c => ComposeSection(c, "Active Rehabilitation Program", inner =>
            {
                if (report.ActiveProgram is null)
                {
                    inner.Item().Text("No active rehabilitation program currently assigned.").Italic();
                    return;
                }

                inner.Item().Text($"{report.ActiveProgram.ProgramTitle} (Prescribed: {report.ActiveProgram.StartDate:yyyy-MM-dd}" +
                    (report.ActiveProgram.EndDate.HasValue ? $" to {report.ActiveProgram.EndDate:yyyy-MM-dd}" : " · Ongoing") + ")").Bold();

                if (report.ActiveProgram.Exercises.Count > 0)
                {
                    inner.Item().PaddingTop(6).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.ConstantColumn(60);
                            columns.ConstantColumn(50);
                            columns.ConstantColumn(60);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(NeutralGrey).Padding(3).Text("Prescribed Exercise").Bold().FontSize(8.5f);
                            header.Cell().Background(NeutralGrey).Padding(3).Text("Reps").Bold().FontSize(8.5f);
                            header.Cell().Background(NeutralGrey).Padding(3).Text("Sets").Bold().FontSize(8.5f);
                            header.Cell().Background(NeutralGrey).Padding(3).Text("Freq/Day").Bold().FontSize(8.5f);
                        });

                        foreach (var ex in report.ActiveProgram.Exercises)
                        {
                            table.Cell().BorderBottom(1).BorderColor(NeutralGrey).Padding(3).Text(ex.Title).FontSize(8.5f);
                            table.Cell().BorderBottom(1).BorderColor(NeutralGrey).Padding(3).Text(ex.Repetitions.ToString()).FontSize(8.5f);
                            table.Cell().BorderBottom(1).BorderColor(NeutralGrey).Padding(3).Text(ex.Sets.ToString()).FontSize(8.5f);
                            table.Cell().BorderBottom(1).BorderColor(NeutralGrey).Padding(3).Text(ex.FrequencyPerDay.ToString()).FontSize(8.5f);
                        }
                    });
                }
            }));

            // Clinical Progress & Outcomes
            column.Item().Element(c => ComposeSection(c, "Clinical Progress & Narrative Assessment", inner =>
            {
                inner.Item().Row(row =>
                {
                    row.RelativeItem().Text($"Tracked Sessions: {report.TotalCompletedSessions} of {report.TotalTrackedDays} logged").FontSize(9).Bold();
                    if (report.InitialPainScore.HasValue && report.FinalPainScore.HasValue)
                    {
                        row.RelativeItem().Text($"Pain Trend: {report.InitialPainScore}/10 → {report.FinalPainScore}/10").FontSize(9).Bold().FontColor(AccentSage);
                    }
                    if (report.InitialMobilityScore.HasValue && report.FinalMobilityScore.HasValue)
                    {
                        row.RelativeItem().Text($"Mobility Trend: {report.InitialMobilityScore}/10 → {report.FinalMobilityScore}/10").FontSize(9).Bold().FontColor(AccentSage);
                    }
                });

                inner.Item().PaddingTop(4).Text(report.NarrativeSummary).FontSize(9);
            }));

            // Referenced Clinical Sessions & Appointment Notes
            ComposeReferencedSessions(column, report.ReferencedSessions);

            // Tracking Log History
            column.Item().Element(c => ComposeSection(c, "Daily Tracking & Recovery Log History", inner =>
            {
                if (report.Logs.Count == 0)
                {
                    inner.Item().Text("No daily tracking entries recorded yet.").Italic().FontSize(8.5f);
                    return;
                }

                inner.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(65);
                        columns.ConstantColumn(35);
                        columns.ConstantColumn(35);
                        columns.ConstantColumn(35);
                        columns.ConstantColumn(35);
                        columns.ConstantColumn(35);
                        columns.ConstantColumn(40);
                        columns.ConstantColumn(40);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(NeutralGrey).Padding(2.5f).Text("Date").Bold().FontSize(8);
                        header.Cell().Background(NeutralGrey).Padding(2.5f).Text("Pain").Bold().FontSize(8);
                        header.Cell().Background(NeutralGrey).Padding(2.5f).Text("Mob").Bold().FontSize(8);
                        header.Cell().Background(NeutralGrey).Padding(2.5f).Text("Eng").Bold().FontSize(8);
                        header.Cell().Background(NeutralGrey).Padding(2.5f).Text("App").Bold().FontSize(8);
                        header.Cell().Background(NeutralGrey).Padding(2.5f).Text("Lam").Bold().FontSize(8);
                        header.Cell().Background(NeutralGrey).Padding(2.5f).Text("Weight").Bold().FontSize(8);
                        header.Cell().Background(NeutralGrey).Padding(2.5f).Text("Done").Bold().FontSize(8);
                    });

                    foreach (var log in report.Logs.TakeLast(10))
                    {
                        table.Cell().BorderBottom(1).BorderColor(NeutralGrey).Padding(2.5f).Text(log.LogDate.ToString("yyyy-MM-dd")).FontSize(8);
                        table.Cell().BorderBottom(1).BorderColor(NeutralGrey).Padding(2.5f).Text(FormatScore(log.PainScore)).FontSize(8);
                        table.Cell().BorderBottom(1).BorderColor(NeutralGrey).Padding(2.5f).Text(FormatScore(log.MobilityScore)).FontSize(8);
                        table.Cell().BorderBottom(1).BorderColor(NeutralGrey).Padding(2.5f).Text(FormatScore(log.EnergyScore)).FontSize(8);
                        table.Cell().BorderBottom(1).BorderColor(NeutralGrey).Padding(2.5f).Text(FormatScore(log.AppetiteScore)).FontSize(8);
                        table.Cell().BorderBottom(1).BorderColor(NeutralGrey).Padding(2.5f).Text(FormatScore(log.LamenessScore)).FontSize(8);
                        table.Cell().BorderBottom(1).BorderColor(NeutralGrey).Padding(2.5f).Text(log.WeightKg?.ToString("0.#") ?? "—").FontSize(8);
                        table.Cell().BorderBottom(1).BorderColor(NeutralGrey).Padding(2.5f).Text(log.IsCompleted ? "Yes" : "No").FontSize(8);
                    }
                });
            }));
        });
    }

    // ================= DISCHARGE SUMMARY LAYOUT =================
    private static void ComposeDischargeContent(IContainer container, PetClinicalReportDto report)
    {
        container.PaddingTop(12).Column(column =>
        {
            column.Spacing(12);

            // Patient Information & Referral
            column.Item().Element(c => ComposeSection(c, "Patient & Referral Overview", inner =>
            {
                inner.Item().Row(row =>
                {
                    row.RelativeItem().Column(c1 =>
                    {
                        c1.Item().Text($"Patient: {report.PetName}").Bold();
                        c1.Item().Text($"Species: {report.Species}{(report.Breed is not null ? $" · {report.Breed}" : "")}");
                        if (report.WeightKg.HasValue) c1.Item().Text($"Discharge Weight: {report.WeightKg:0.##} kg");
                    });
                    row.RelativeItem().Column(c2 =>
                    {
                        c2.Item().Text($"Owner: {report.OwnerName}").Bold();
                        c2.Item().Text($"Initial Diagnosis: {report.Diagnosis ?? "Musculoskeletal rehabilitation"}");
                        c2.Item().Text($"Clinician: {report.PhysioName ?? "Triple A Veterinary Physiotherapy"}");
                    });
                });
            }));

            // Discharge Status Banner
            var statusText = report.DischargeStatus ?? "Rehabilitation Goals Achieved — Formally Discharged to Home Maintenance";
            column.Item().Background(AccentSageLight).Padding(8).Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("Discharge Status:").FontSize(9).Bold().FontColor(AccentSage);
                    c.Item().Text(statusText).FontSize(11).Bold().FontColor(PrimaryDark);
                });
                row.ConstantItem(120).AlignRight().Column(c =>
                {
                    c.Item().Text($"Total Completed: {report.TotalCompletedSessions} sessions").FontSize(9).Bold().FontColor(PrimaryDark);
                    c.Item().Text($"Tracked: {report.TotalTrackedDays} days").FontSize(8.5f).FontColor(NeutralMuted);
                });
            });

            // Comparative Clinical Outcomes
            column.Item().Element(c => ComposeSection(c, "Comparative Outcome Measures (Initial vs Final Discharge)", inner =>
            {
                inner.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(1.5f);
                        columns.RelativeColumn(1.5f);
                        columns.RelativeColumn(2);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(NeutralGrey).Padding(3.5f).Text("Clinical Metric").Bold().FontSize(8.5f);
                        header.Cell().Background(NeutralGrey).Padding(3.5f).Text("Initial Assessment").Bold().FontSize(8.5f);
                        header.Cell().Background(NeutralGrey).Padding(3.5f).Text("Discharge Assessment").Bold().FontSize(8.5f);
                        header.Cell().Background(NeutralGrey).Padding(3.5f).Text("Outcome & Recovery").Bold().FontSize(8.5f);
                    });

                    // Pain Score
                    var initPain = report.InitialPainScore ?? 6;
                    var finalPain = report.FinalPainScore ?? 1;
                    table.Cell().BorderBottom(1).BorderColor(NeutralGrey).Padding(3.5f).Text("Pain Rating (0-10)").FontSize(8.5f);
                    table.Cell().BorderBottom(1).BorderColor(NeutralGrey).Padding(3.5f).Text($"{initPain} / 10").FontSize(8.5f);
                    table.Cell().BorderBottom(1).BorderColor(NeutralGrey).Padding(3.5f).Text($"{finalPain} / 10").FontSize(8.5f).Bold().FontColor(AccentSage);
                    table.Cell().BorderBottom(1).BorderColor(NeutralGrey).Padding(3.5f).Text(finalPain < initPain ? "Significant pain reduction" : "Well controlled").FontSize(8.5f);

                    // Mobility Score
                    var initMob = report.InitialMobilityScore ?? 3;
                    var finalMob = report.FinalMobilityScore ?? 9;
                    table.Cell().BorderBottom(1).BorderColor(NeutralGrey).Padding(3.5f).Text("Functional Mobility (0-10)").FontSize(8.5f);
                    table.Cell().BorderBottom(1).BorderColor(NeutralGrey).Padding(3.5f).Text($"{initMob} / 10").FontSize(8.5f);
                    table.Cell().BorderBottom(1).BorderColor(NeutralGrey).Padding(3.5f).Text($"{finalMob} / 10").FontSize(8.5f).Bold().FontColor(AccentSage);
                    table.Cell().BorderBottom(1).BorderColor(NeutralGrey).Padding(3.5f).Text("Substantial mobility gain").FontSize(8.5f);

                    // Lameness Score
                    var initLam = report.InitialLamenessScore ?? 3;
                    var finalLam = report.FinalLamenessScore ?? 0;
                    table.Cell().BorderBottom(1).BorderColor(NeutralGrey).Padding(3.5f).Text("Lameness Grade (0-5)").FontSize(8.5f);
                    table.Cell().BorderBottom(1).BorderColor(NeutralGrey).Padding(3.5f).Text($"Grade {initLam} / 5").FontSize(8.5f);
                    table.Cell().BorderBottom(1).BorderColor(NeutralGrey).Padding(3.5f).Text($"Grade {finalLam} / 5").FontSize(8.5f).Bold().FontColor(AccentSage);
                    table.Cell().BorderBottom(1).BorderColor(NeutralGrey).Padding(3.5f).Text(finalLam == 0 ? "Lameness fully resolved" : "Minimal residual lameness").FontSize(8.5f);
                });
            }));

            // Referenced Clinical Sessions & Appointment Notes
            ComposeReferencedSessions(column, report.ReferencedSessions);

            // Long-Term Home Maintenance Plan
            column.Item().Element(c => ComposeSection(c, "Long-Term Home Maintenance Protocol", inner =>
            {
                var maintenance = !string.IsNullOrWhiteSpace(report.MaintenancePlan)
                    ? report.MaintenancePlan
                    : $"1. Maintain 2x daily controlled leash walks (20-30 minutes per walk).\n" +
                      $"2. Continue gentle stretching and core stabilization exercises 3 times per week.\n" +
                      $"3. Maintain healthy body weight to minimize joint load.\n" +
                      $"4. Avoid high-impact concussive activities (jumping from elevated heights or rough agility).";

                inner.Item().Text(maintenance).FontSize(9);
            }));

            // Instructions for Referring Veterinarian & Owner
            column.Item().Element(c => ComposeSection(c, "Veterinary & Owner Recommendations", inner =>
            {
                var vetNotes = !string.IsNullOrWhiteSpace(report.VeterinarianNotes)
                    ? report.VeterinarianNotes
                    : $"{report.PetName} has completed their course of veterinary physical rehabilitation with excellent functional outcomes. " +
                      $"The surgical/injured site shows stable joint extension, resolved muscle atrophy, and no effusion. " +
                      $"Recommend routine veterinary recheck at 6 months. Advise immediate reassessment should any acute lameness recur.";

                inner.Item().Text(vetNotes).FontSize(9);
            }));

            // Clinician Sign-off
            column.Item().PaddingTop(4).Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("Clinician Sign-off:").FontSize(8.5f).Bold().FontColor(NeutralMuted);
                    c.Item().Text(report.PhysioName ?? "Triple A Veterinary Physiotherapy").FontSize(9.5f).Bold().FontColor(PrimaryDark);
                    c.Item().Text("Licensed Veterinary Physiotherapist").FontSize(8).FontColor(NeutralMuted);
                });
                row.ConstantItem(140).AlignRight().Column(c =>
                {
                    c.Item().Text("Discharge Date:").FontSize(8.5f).Bold().FontColor(NeutralMuted);
                    c.Item().Text($"{DateTime.UtcNow:yyyy-MM-dd}").FontSize(9.5f).Bold().FontColor(PrimaryDark);
                });
            });
        });
    }

    // ================= OWNER HOME PROGRAM LAYOUT =================
    private static void ComposeHomeProgramContent(IContainer container, PetClinicalReportDto report)
    {
        container.PaddingTop(12).Column(column =>
        {
            column.Spacing(12);

            // Program Banner
            column.Item().Background(AccentSageLight).Padding(8).Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text($"Home Rehabilitation Guide for {report.PetName}").FontSize(12).Bold().FontColor(PrimaryDark);
                    c.Item().Text($"Prepared for {report.OwnerName} · Diagnosis: {report.Diagnosis ?? "Rehabilitation"}").FontSize(8.5f).FontColor(NeutralDark);
                });
            });

            // How to Conduct Home Exercises
            column.Item().Element(c => ComposeSection(c, "Guidelines for Safe & Effective Home Exercises", inner =>
            {
                inner.Item().Text(
                    "• Warm Up: Walk gently for 3-5 minutes on a flat surface or apply gentle warm compress before starting.\n" +
                    "• Non-Slip Flooring: Always perform exercises on a rug, yoga mat, or grassy area with secure footing.\n" +
                    "• Positive Reinforcement: Use favorite treats and calm praise. Keep sessions encouraging and fun.\n" +
                    "• Rest Breaks: Provide 30-60 seconds of rest between exercise sets."
                ).FontSize(8.5f);
            }));

            // Prescribed Exercises
            column.Item().Element(c => ComposeSection(c, "Prescribed Home Exercises & Technique", inner =>
            {
                if (report.ActiveProgram?.Exercises != null && report.ActiveProgram.Exercises.Count > 0)
                {
                    inner.Item().Column(col =>
                    {
                        col.Spacing(6);
                        foreach (var ex in report.ActiveProgram.Exercises)
                        {
                            col.Item().Border(1).BorderColor(NeutralGrey).Padding(6).Column(ec =>
                            {
                                ec.Item().Row(r =>
                                {
                                    r.RelativeItem().Text(ex.Title).FontSize(9.5f).Bold().FontColor(PrimaryDark);
                                    r.ConstantItem(160).AlignRight().Text($"{ex.Repetitions} reps · {ex.Sets} sets · {ex.FrequencyPerDay}x/day").FontSize(8.5f).Bold().FontColor(AccentSage);
                                });
                                ec.Item().PaddingTop(2).Text($"Instructions: Perform in a controlled manner. Ensure proper alignment and reward your pet upon completion.").FontSize(8f).FontColor(NeutralDark);
                            });
                        }
                    });
                }
                else
                {
                    inner.Item().Text("1. Passive Range of Motion (PROM): 10 reps, 2 sets, 2x daily. Gently flex and extend joint within comfortable limits.\n" +
                                     "2. Weight Shifting / Balance Stand: 10 reps (hold 5s), 2 sets, 2x daily. Gently shift weight side to side on non-slip mat.\n" +
                                     "3. Controlled Leash Walking: 15-20 minutes, 2x daily on flat surfaces at a slow, deliberate pace.").FontSize(8.5f);
                }
            }));

            // Dos and Don'ts / Safety Precautions
            column.Item().Element(c => ComposeSection(c, "Important Safety Rules & Precautions", inner =>
            {
                inner.Item().Row(row =>
                {
                    row.RelativeItem().Border(1).BorderColor(AccentSage).Padding(6).Column(c =>
                    {
                        c.Item().Text("DO:").Bold().FontSize(8.5f).FontColor(AccentSage);
                        c.Item().Text("✓ Monitor your pet for signs of comfort and steady breathing\n✓ Keep walks strictly on a short leash\n✓ Log exercise completion in the Owner Portal").FontSize(8f);
                    });

                    row.ConstantItem(8);

                    row.RelativeItem().Border(1).BorderColor(Colors.Red.Medium).Padding(6).Column(c =>
                    {
                        c.Item().Text("DON'T:").Bold().FontSize(8.5f).FontColor(Colors.Red.Medium);
                        c.Item().Text("✗ Never force movement if your pet resists or whimpers\n✗ Avoid slippery hardwood, tile, or laminate floors\n✗ Do not allow jumping onto couches, beds, or car seats").FontSize(8f);
                    });
                });
            }));

            // Contact & Help
            column.Item().Element(c => ComposeSection(c, "When to Contact the Clinic", inner =>
            {
                inner.Item().Text(
                    "If you notice sudden reluctance to bear weight, increased joint swelling, or whimpering during movement, stop the exercises and message the Triple A team or your veterinarian immediately."
                ).FontSize(8.5f).Italic();
            }));
        });
    }

    private static void ComposeReferencedSessions(ColumnDescriptor column, IReadOnlyList<ReferencedReportSessionDto>? sessions)
    {
        if (sessions is null || sessions.Count == 0) return;

        column.Item().Element(c => ComposeSection(c, "Referenced Clinical Sessions & Appointment Notes", inner =>
        {
            inner.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(75);
                    columns.ConstantColumn(105);
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(3);
                });

                table.Header(header =>
                {
                    header.Cell().Background(AccentSageLight).Padding(3.5f).Text("Date").Bold().FontSize(8);
                    header.Cell().Background(AccentSageLight).Padding(3.5f).Text("Session Type").Bold().FontSize(8);
                    header.Cell().Background(AccentSageLight).Padding(3.5f).Text("Clinical Findings").Bold().FontSize(8);
                    header.Cell().Background(AccentSageLight).Padding(3.5f).Text("Clinician Comments").Bold().FontSize(8);
                });

                foreach (var session in sessions)
                {
                    table.Cell().BorderBottom(1).BorderColor(NeutralGrey).Padding(3.5f)
                        .Text(session.Date.ToString("yyyy-MM-dd")).FontSize(7.5f);
                    table.Cell().BorderBottom(1).BorderColor(NeutralGrey).Padding(3.5f)
                        .Text(session.SessionType).FontSize(7.5f).SemiBold();
                    table.Cell().BorderBottom(1).BorderColor(NeutralGrey).Padding(3.5f)
                        .Text(session.SessionNotes ?? "—").FontSize(7.5f);
                    table.Cell().BorderBottom(1).BorderColor(NeutralGrey).Padding(3.5f)
                        .Text(session.ClinicianComment ?? "—").FontSize(7.5f).Italic();
                }
            });
        }));
    }

    private static void ComposeSection(IContainer container, string title, Action<ColumnDescriptor> content)
    {
        container.Column(column =>
        {
            column.Item().Text(title).FontSize(10.5f).Bold().FontColor(PrimaryDark);
            column.Item().PaddingTop(4).Column(content);
        });
    }

    private static string FormatScore(int? score) => score?.ToString() ?? "—";
}
