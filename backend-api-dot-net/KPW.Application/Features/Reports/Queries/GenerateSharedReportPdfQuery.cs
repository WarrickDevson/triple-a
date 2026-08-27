using KPW.Application.DTOs.Progress;
using KPW.Application.DTOs.Reports;
using KPW.Application.Features.Pets;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using KPW.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.Reports.Queries;

public record GenerateSharedReportPdfQuery(int SharedReportId) : IRequest<PetReportFileDto>;

public class GenerateSharedReportPdfQueryHandler : IRequestHandler<GenerateSharedReportPdfQuery, PetReportFileDto>
{
    private readonly DbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPetReportPdfGenerator _pdfGenerator;
    private readonly ISoapReportPdfGenerator _soapPdfGenerator;

    public GenerateSharedReportPdfQueryHandler(
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

    public async Task<PetReportFileDto> Handle(GenerateSharedReportPdfQuery query, CancellationToken cancellationToken)
    {
        if (_currentUserService.Role is not (UserRole.Physio or UserRole.SysAdmin or UserRole.Owner))
        {
            throw new UnauthorizedAccessException("You are not authorized to download this clinical report.");
        }

        var sharedReport = await _dbContext.Set<SharedReport>()
            .AsNoTracking()
            .Include(r => r.SharedByPhysio)
            .Include(r => r.SoapNote)
                .ThenInclude(s => s!.Physio)
            .Include(r => r.Pet)
                .ThenInclude(p => p.Owner)
            .Include(r => r.Pet)
                .ThenInclude(p => p.MedicalHistories)
            .FirstOrDefaultAsync(r => r.SharedReportId == query.SharedReportId, cancellationToken);

        if (sharedReport is null)
        {
            throw new KeyNotFoundException($"Report with ID {query.SharedReportId} not found.");
        }

        if (_currentUserService.Role == UserRole.Owner && !sharedReport.IsActive)
        {
            throw new KeyNotFoundException($"Report with ID {query.SharedReportId} not found.");
        }

        await PetAuthorization.EnsureCanAccessPet(_dbContext, _currentUserService, sharedReport.PetId, cancellationToken);

        var pet = sharedReport.Pet;
        var safeName = SanitizeFileName(pet.PetName);
        var dateStr = sharedReport.SharedAtUtc.ToString("yyyyMMdd");

        // If it's a SOAP note report and has a linked soap note, generate the SOAP PDF
        if (sharedReport.ReportType.Equals("SOAP_SESSION", StringComparison.OrdinalIgnoreCase) && sharedReport.SoapNote is not null)
        {
            var soapDto = SoapNotes.SoapNoteMapper.ToDto(sharedReport.SoapNote);
            var ownerName = $"{pet.Owner.FirstName} {pet.Owner.LastName}".Trim();
            var soapBytes = _soapPdfGenerator.Generate(soapDto, pet.PetName, pet.Species, pet.Breed, ownerName);
            var soapFileName = $"TripleA-SoapReport-{safeName}-{dateStr}.pdf";
            return new PetReportFileDto(soapBytes, soapFileName);
        }

        var latestHistory = pet.MedicalHistories.OrderByDescending(m => m.CreatedDate).FirstOrDefault();

        var activeProgram = await _dbContext.Set<RehabProgram>()
            .AsNoTracking()
            .Include(p => p.RehabProgramExercises)
                .ThenInclude(e => e.Exercise)
            .Where(p => p.PetId == pet.PetId)
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
            .Where(l => l.PetId == pet.PetId)
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

        var typeLower = (sharedReport.ReportType ?? string.Empty).ToLowerInvariant();
        var typeKey = typeLower.Contains("discharge") ? "discharge" : typeLower.Contains("home") ? "home-program" : typeLower.Contains("soap") ? "soap" : "progress";

        var physioName = sharedReport.SharedByPhysio != null
            ? $"{sharedReport.SharedByPhysio.FirstName} {sharedReport.SharedByPhysio.LastName}".Trim()
            : "Clinician";

        var pastAppointments = await _dbContext.Set<Appointment>()
            .AsNoTracking()
            .Where(a => a.PetId == pet.PetId)
            .OrderByDescending(a => a.ScheduledDateTime)
            .Take(4)
            .ToListAsync(cancellationToken);

        var pastSoapNotes = await _dbContext.Set<SoapNote>()
            .AsNoTracking()
            .Where(s => s.PetId == pet.PetId)
            .OrderByDescending(s => s.SessionDate)
            .Take(4)
            .ToListAsync(cancellationToken);

        var referencedSessions = new List<ReferencedReportSessionDto>();
        foreach (var appt in pastAppointments)
        {
            referencedSessions.Add(new ReferencedReportSessionDto(
                appt.ScheduledDateTime,
                "Physiotherapy Consultation",
                appt.ClinicianNotes ?? appt.ClientNotes ?? "Physical therapy evaluation and treatment.",
                null));
        }

        foreach (var soap in pastSoapNotes)
        {
            if (!referencedSessions.Any(r => r.Date.Date == soap.SessionDate.Date))
            {
                referencedSessions.Add(new ReferencedReportSessionDto(
                    soap.SessionDate,
                    "Clinical SOAP Evaluation",
                    $"Assessment: {soap.Action}. Plan: {soap.Plan}",
                    null));
            }
        }

        referencedSessions = referencedSessions.OrderByDescending(r => r.Date).Take(5).ToList();

        var reportDto = new PetClinicalReportDto(
            pet.PetId,
            pet.PetName,
            $"{pet.Owner.FirstName} {pet.Owner.LastName}".Trim(),
            pet.Species,
            pet.Breed,
            pet.WeightKg,
            latestHistory?.Diagnosis,
            latestHistory?.InjuryOrCondition,
            programDto,
            logs.Count(l => l.IsCompleted),
            logs.Count,
            logs,
            sharedReport.Summary ?? string.Empty,
            ReportType: typeKey,
            CustomTitle: sharedReport.Title,
            CustomSummary: sharedReport.Summary,
            PhysioName: physioName,
            InitialPainScore: painLogs.FirstOrDefault()?.PainScore,
            FinalPainScore: painLogs.LastOrDefault()?.PainScore,
            InitialMobilityScore: mobilityLogs.FirstOrDefault()?.MobilityScore,
            FinalMobilityScore: mobilityLogs.LastOrDefault()?.MobilityScore,
            InitialLamenessScore: lamenessLogs.FirstOrDefault()?.LamenessScore,
            FinalLamenessScore: lamenessLogs.LastOrDefault()?.LamenessScore,
            ReferencedSessions: referencedSessions);

        var pdfBytes = _pdfGenerator.Generate(reportDto);
        var fileName = typeKey switch
        {
            "discharge" => $"TripleA-DischargeSummary-{safeName}-{dateStr}.pdf",
            "home-program" => $"TripleA-HomeProgram-{safeName}-{dateStr}.pdf",
            _ => $"TripleA-ProgressReport-{safeName}-{dateStr}.pdf"
        };

        return new PetReportFileDto(pdfBytes, fileName);
    }

    private static string SanitizeFileName(string name) =>
        string.Concat(name.Split(Path.GetInvalidFileNameChars())).Replace(' ', '-');
}
