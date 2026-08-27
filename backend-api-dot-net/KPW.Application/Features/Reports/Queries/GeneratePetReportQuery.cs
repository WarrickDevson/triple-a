using KPW.Application.DTOs.Progress;
using KPW.Application.DTOs.Reports;
using KPW.Application.Features.Pets;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using KPW.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.Reports.Queries;

public record GeneratePetReportQuery(
    int PetId,
    string? ReportType = null,
    string? CustomTitle = null,
    string? CustomSummary = null,
    string? DischargeStatus = null,
    string? MaintenancePlan = null,
    string? VeterinarianNotes = null,
    string? OwnerInstructions = null,
    int? SoapNoteId = null) : IRequest<PetReportFileDto>;

public class GeneratePetReportQueryHandler : IRequestHandler<GeneratePetReportQuery, PetReportFileDto>
{
    private readonly DbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPetReportPdfGenerator _pdfGenerator;
    private readonly ISoapReportPdfGenerator _soapPdfGenerator;

    public GeneratePetReportQueryHandler(
        DbContext dbContext,
        ICurrentUserService currentUserService,
        IPetReportPdfGenerator pdfGenerator,
        ISoapReportPdfGenerator soapPdfGenerator)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _pdfGenerator = pdfGenerator;
        _soapPdfGenerator = soapPdfGenerator;
    }

    public async Task<PetReportFileDto> Handle(GeneratePetReportQuery query, CancellationToken cancellationToken)
    {
        if (_currentUserService.Role is not (UserRole.Physio or UserRole.SysAdmin or UserRole.Owner))
        {
            throw new UnauthorizedAccessException("You are not authorized to download clinical reports.");
        }

        await PetAuthorization.EnsureCanAccessPet(
            _dbContext, _currentUserService, query.PetId, cancellationToken);

        var pet = await _dbContext.Set<Pet>()
            .AsNoTracking()
            .Include(p => p.Owner)
            .Include(p => p.MedicalHistories)
            .FirstAsync(p => p.PetId == query.PetId, cancellationToken);

        var normalizedType = (query.ReportType ?? "progress").Trim().ToLowerInvariant() switch
        {
            var t when t.Contains("discharge") => "discharge",
            var t when t.Contains("home") => "home-program",
            var t when t.Contains("soap") => "soap",
            _ => "progress"
        };

        var safeName = SanitizeFileName(pet.PetName);
        var dateStr = DateTime.UtcNow.ToString("yyyyMMdd");

        // If specific SOAP note requested
        if (normalizedType == "soap" && query.SoapNoteId.HasValue)
        {
            var soapNote = await _dbContext.Set<SoapNote>()
                .AsNoTracking()
                .Include(s => s.Physio)
                .FirstOrDefaultAsync(s => s.SoapNoteId == query.SoapNoteId.Value && s.PetId == query.PetId, cancellationToken);

            if (soapNote != null)
            {
                var soapDto = SoapNotes.SoapNoteMapper.ToDto(soapNote);
                var ownerName = $"{pet.Owner.FirstName} {pet.Owner.LastName}".Trim();
                var soapBytes = _soapPdfGenerator.Generate(soapDto, pet.PetName, pet.Species, pet.Breed, ownerName);
                return new PetReportFileDto(soapBytes, $"TripleA-SoapReport-{safeName}-{dateStr}.pdf");
            }
        }

        var latestHistory = pet.MedicalHistories
            .OrderByDescending(m => m.CreatedDate)
            .FirstOrDefault();

        var activeProgram = await _dbContext.Set<RehabProgram>()
            .AsNoTracking()
            .Include(p => p.RehabProgramExercises)
                .ThenInclude(e => e.Exercise)
            .Where(p => p.PetId == query.PetId)
            .OrderByDescending(p => p.StartDate)
            .FirstOrDefaultAsync(cancellationToken);

        RehabProgramReportDto? programDto = null;
        if (activeProgram is not null)
        {
            programDto = new RehabProgramReportDto(
                activeProgram.ProgramTitle,
                activeProgram.StartDate,
                activeProgram.EndDate,
                activeProgram.RehabProgramExercises
                    .OrderBy(e => e.RehabProgramExerciseId)
                    .Select(e => new RehabProgramExerciseReportDto(
                        e.Exercise.Title,
                        e.Repetitions,
                        e.Sets,
                        e.FrequencyPerDay))
                    .ToList());
        }

        var logs = await _dbContext.Set<DailyTrackingLog>()
            .AsNoTracking()
            .Where(l => l.PetId == query.PetId)
            .OrderBy(l => l.LogDate)
            .Select(l => new PetProgressLogDto(
                l.LogDate,
                l.PainScore,
                l.LamenessScore,
                l.EnergyScore,
                l.AppetiteScore,
                l.MobilityScore,
                l.WeightKg,
                l.IsCompleted))
            .ToListAsync(cancellationToken);

        var painLogs = logs.Where(l => l.PainScore.HasValue).ToList();
        var mobilityLogs = logs.Where(l => l.MobilityScore.HasValue).ToList();
        var lamenessLogs = logs.Where(l => l.LamenessScore.HasValue).ToList();

        var narrativeSummary = !string.IsNullOrWhiteSpace(query.CustomSummary)
            ? query.CustomSummary
            : BuildNarrativeSummary(logs, normalizedType, pet.PetName, latestHistory?.Diagnosis);

        var physioName = _currentUserService.Role == UserRole.Physio
            ? "Dr. S. Devson, Lead Veterinary Physiotherapist"
            : "Triple A Veterinary Physiotherapy Team";

        var report = new PetClinicalReportDto(
            pet.PetId,
            pet.PetName,
            $"{pet.Owner.FirstName} {pet.Owner.LastName}",
            pet.Species,
            pet.Breed,
            pet.WeightKg,
            latestHistory?.Diagnosis,
            latestHistory?.InjuryOrCondition,
            programDto,
            logs.Count(l => l.IsCompleted),
            logs.Count,
            logs,
            narrativeSummary,
            ReportType: normalizedType,
            CustomTitle: query.CustomTitle,
            CustomSummary: query.CustomSummary,
            DischargeStatus: query.DischargeStatus,
            MaintenancePlan: query.MaintenancePlan,
            VeterinarianNotes: query.VeterinarianNotes,
            OwnerInstructions: query.OwnerInstructions,
            PhysioName: physioName,
            InitialPainScore: painLogs.FirstOrDefault()?.PainScore,
            FinalPainScore: painLogs.LastOrDefault()?.PainScore,
            InitialMobilityScore: mobilityLogs.FirstOrDefault()?.MobilityScore,
            FinalMobilityScore: mobilityLogs.LastOrDefault()?.MobilityScore,
            InitialLamenessScore: lamenessLogs.FirstOrDefault()?.LamenessScore,
            FinalLamenessScore: lamenessLogs.LastOrDefault()?.LamenessScore);

        var pdfBytes = _pdfGenerator.Generate(report);
        var fileName = normalizedType switch
        {
            "discharge" => $"TripleA-DischargeSummary-{safeName}-{dateStr}.pdf",
            "home-program" => $"TripleA-HomeProgram-{safeName}-{dateStr}.pdf",
            "soap" => $"TripleA-SoapSummary-{safeName}-{dateStr}.pdf",
            _ => $"TripleA-ProgressReport-{safeName}-{dateStr}.pdf"
        };

        return new PetReportFileDto(pdfBytes, fileName);
    }

    private static string BuildNarrativeSummary(IReadOnlyList<PetProgressLogDto> logs, string reportType, string petName, string? diagnosis)
    {
        if (reportType == "discharge")
        {
            return $"{petName} has successfully completed the prescribed rehabilitation course for {diagnosis ?? "clinical condition"}. Significant functional mobility gains and pain reduction have been achieved. Patient is formally discharged to the long-term home maintenance regimen.";
        }

        if (reportType == "home-program")
        {
            return $"This customized home exercise guide is designed for {petName}'s ongoing rehabilitation. Follow daily exercise recommendations, observe prescribed repetitions and sets, and monitor comfort levels during all activities.";
        }

        if (logs.Count < 2)
        {
            return $"Rehabilitation tracking for {petName} is actively underway. Continue daily exercise logging and session attendance for optimal trend insights.";
        }

        var painLogs = logs.Where(l => l.PainScore.HasValue).ToList();
        var mobilityLogs = logs.Where(l => l.MobilityScore.HasValue).ToList();

        var parts = new List<string>();

        if (painLogs.Count >= 2)
        {
            var firstPain = painLogs.First().PainScore!.Value;
            var lastPain = painLogs.Last().PainScore!.Value;
            var painTrend = lastPain < firstPain ? "significantly improving" : lastPain > firstPain ? "elevated" : "stable";
            parts.Add($"Pain score trend is {painTrend} (initially {firstPain}/10, currently {lastPain}/10)");
        }

        if (mobilityLogs.Count >= 2)
        {
            var firstMobility = mobilityLogs.First().MobilityScore!.Value;
            var lastMobility = mobilityLogs.Last().MobilityScore!.Value;
            var mobilityTrend = lastMobility > firstMobility ? "improving with enhanced weight bearing" : lastMobility < firstMobility ? "guarded" : "stable";
            parts.Add($"functional mobility is {mobilityTrend} (from {firstMobility}/10 to {lastMobility}/10)");
        }

        return string.Join("; ", parts) + ". Clinical trajectory remains positive with consistent home exercise adherence.";
    }

    private static string SanitizeFileName(string name) =>
        string.Concat(name.Split(Path.GetInvalidFileNameChars())).Replace(' ', '-');
}
