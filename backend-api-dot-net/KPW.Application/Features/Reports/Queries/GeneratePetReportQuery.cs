using KPW.Application.DTOs.Progress;
using KPW.Application.DTOs.Reports;
using KPW.Application.Features.Pets;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using KPW.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.Reports.Queries;

public record GeneratePetReportQuery(int PetId) : IRequest<PetReportFileDto>;

public class GeneratePetReportQueryHandler : IRequestHandler<GeneratePetReportQuery, PetReportFileDto>
{
    private readonly DbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPetReportPdfGenerator _pdfGenerator;

    public GeneratePetReportQueryHandler(
        DbContext dbContext,
        ICurrentUserService currentUserService,
        IPetReportPdfGenerator pdfGenerator)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _pdfGenerator = pdfGenerator;
    }

    public async Task<PetReportFileDto> Handle(GeneratePetReportQuery query, CancellationToken cancellationToken)
    {
        if (_currentUserService.Role is not (UserRole.Physio or UserRole.SysAdmin))
        {
            throw new UnauthorizedAccessException("Only physiotherapists can download clinical reports.");
        }

        await PetAuthorization.EnsureCanAccessPet(
            _dbContext, _currentUserService, query.PetId, cancellationToken);

        var pet = await _dbContext.Set<Pet>()
            .AsNoTracking()
            .Include(p => p.Owner)
            .Include(p => p.MedicalHistories)
            .FirstAsync(p => p.PetId == query.PetId, cancellationToken);

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
            BuildNarrativeSummary(logs));

        var pdfBytes = _pdfGenerator.Generate(report);
        var safeName = SanitizeFileName(pet.PetName);
        var fileName = $"KPW-Report-{safeName}-{DateTime.UtcNow:yyyyMMdd}.pdf";

        return new PetReportFileDto(pdfBytes, fileName);
    }

    private static string BuildNarrativeSummary(IReadOnlyList<PetProgressLogDto> logs)
    {
        if (logs.Count < 2)
        {
            return "Insufficient tracking data for trend analysis. Continue daily logging for meaningful progress insights.";
        }

        var painLogs = logs.Where(l => l.PainScore.HasValue).ToList();
        var mobilityLogs = logs.Where(l => l.MobilityScore.HasValue).ToList();

        if (painLogs.Count < 2 && mobilityLogs.Count < 2)
        {
            return "Insufficient pain and mobility scores for trend analysis.";
        }

        var parts = new List<string>();

        if (painLogs.Count >= 2)
        {
            var firstPain = painLogs.First().PainScore!.Value;
            var lastPain = painLogs.Last().PainScore!.Value;
            var painTrend = lastPain < firstPain ? "improving" : lastPain > firstPain ? "worsening" : "stable";
            parts.Add($"Pain trend is {painTrend} (from {firstPain}/10 to {lastPain}/10)");
        }

        if (mobilityLogs.Count >= 2)
        {
            var firstMobility = mobilityLogs.First().MobilityScore!.Value;
            var lastMobility = mobilityLogs.Last().MobilityScore!.Value;
            var mobilityTrend = lastMobility > firstMobility ? "improving" : lastMobility < firstMobility ? "declining" : "stable";
            parts.Add($"mobility is {mobilityTrend} (from {firstMobility}/10 to {lastMobility}/10)");
        }

        return string.Join("; ", parts) + ".";
    }

    private static string SanitizeFileName(string name) =>
        string.Concat(name.Split(Path.GetInvalidFileNameChars())).Replace(' ', '-');
}
