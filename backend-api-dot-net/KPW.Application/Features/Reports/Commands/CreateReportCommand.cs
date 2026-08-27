using KPW.Application.DTOs.Reports;
using KPW.Application.DTOs.SoapNotes;
using KPW.Application.Features.Pets;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using KPW.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.Reports.Commands;

public record CreateReportCommand(CreateReportRequestDto Request) : IRequest<SharedReportDto>;

public class CreateReportCommandHandler : IRequestHandler<CreateReportCommand, SharedReportDto>
{
    private readonly DbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public CreateReportCommandHandler(DbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<SharedReportDto> Handle(CreateReportCommand command, CancellationToken cancellationToken)
    {
        if (_currentUserService.Role is not (UserRole.Physio or UserRole.SysAdmin))
        {
            throw new UnauthorizedAccessException("Only physiotherapists can generate and publish clinical reports.");
        }

        var req = command.Request;
        await PetAuthorization.EnsureCanAccessPet(_dbContext, _currentUserService, req.PetId, cancellationToken);

        var pet = await _dbContext.Set<Pet>()
            .Include(p => p.Owner)
            .Include(p => p.MedicalHistories)
            .Include(p => p.RehabPrograms)
            .FirstOrDefaultAsync(p => p.PetId == req.PetId, cancellationToken);

        if (pet is null)
        {
            throw new KeyNotFoundException($"Pet with ID {req.PetId} not found.");
        }

        // Standardize report type format
        var normalizedType = req.ReportType.Trim().ToUpperInvariant() switch
        {
            "DISCHARGE" or "DISCHARGE_SUMMARY" => "DISCHARGE_SUMMARY",
            "HOME_PROGRAM" or "OWNER_HOME_PROGRAM" or "HOME-PROGRAM" => "OWNER_HOME_PROGRAM",
            "SOAP" or "SOAP_SESSION" => "SOAP_SESSION",
            "CLINICAL_DOCUMENT" or "DOCUMENT" => "CLINICAL_DOCUMENT",
            _ => "PROGRESS_REPORT"
        };

        var title = !string.IsNullOrWhiteSpace(req.Title)
            ? req.Title.Trim()
            : normalizedType switch
            {
                "DISCHARGE_SUMMARY" => $"Discharge Summary - {pet.PetName} ({DateTime.UtcNow:MMM dd, yyyy})",
                "OWNER_HOME_PROGRAM" => $"Home Exercise Program - {pet.PetName}",
                "SOAP_SESSION" => $"SOAP Session Summary - {pet.PetName}",
                _ => $"Clinical Progress Report - {pet.PetName} ({DateTime.UtcNow:MMM dd, yyyy})"
            };

        var summary = req.Summary?.Trim() ?? string.Empty;

        var periodText = (req.PeriodFrom.HasValue && req.PeriodTo.HasValue)
            ? $"[Period: {req.PeriodFrom:dd MMM yyyy} – {req.PeriodTo:dd MMM yyyy}]\n"
            : req.PeriodFrom.HasValue
                ? $"[Period from: {req.PeriodFrom:dd MMM yyyy}]\n"
                : string.Empty;

        if (!string.IsNullOrEmpty(periodText) && !summary.StartsWith("[Period", StringComparison.OrdinalIgnoreCase))
        {
            summary = $"{periodText}{summary}";
        }

        if (req.ReferencedSessions is { Count: > 0 } && !summary.Contains("Referenced Sessions:", StringComparison.OrdinalIgnoreCase))
        {
            var sessionsSummary = string.Join("\n", req.ReferencedSessions.Select(s =>
                $"• {s.Date:dd MMM yyyy} ({s.SessionType}): {(string.IsNullOrWhiteSpace(s.SessionNotes) ? "Session completed." : s.SessionNotes)}" +
                (!string.IsNullOrWhiteSpace(s.ClinicianComment) ? $" [Clinician Note: {s.ClinicianComment}]" : string.Empty)));

            summary = $"{summary}\n\nReferenced Sessions:\n{sessionsSummary}";
        }

        var sharedReport = new SharedReport
        {
            PetId = req.PetId,
            SoapNoteId = req.SoapNoteId,
            SharedByPhysioId = _currentUserService.UserId!.Value,
            Title = title,
            ReportType = normalizedType,
            Summary = summary,
            SharedAtUtc = DateTime.UtcNow,
            IsActive = req.ShareWithOwner
        };

        _dbContext.Set<SharedReport>().Add(sharedReport);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var created = await _dbContext.Set<SharedReport>()
            .Include(r => r.SharedByPhysio)
            .Include(r => r.Pet)
                .ThenInclude(p => p.Owner)
            .FirstAsync(r => r.SharedReportId == sharedReport.SharedReportId, cancellationToken);

        return SoapNotes.SoapNoteMapper.ToSharedReportDto(created);
    }
}
