using KPW.Application.DTOs.SoapNotes;
using KPW.Application.Features.Pets;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using KPW.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.Reports.Commands;

public record PublishProgressReportCommand(int PetId, string? CustomTitle = null) : IRequest<SharedReportDto>;

public class PublishProgressReportCommandHandler : IRequestHandler<PublishProgressReportCommand, SharedReportDto>
{
    private readonly DbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public PublishProgressReportCommandHandler(DbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<SharedReportDto> Handle(PublishProgressReportCommand command, CancellationToken cancellationToken)
    {
        if (_currentUserService.Role is not (UserRole.Physio or UserRole.SysAdmin))
        {
            throw new UnauthorizedAccessException("Only physiotherapists can publish progress reports to owners.");
        }

        await PetAuthorization.EnsureCanAccessPet(_dbContext, _currentUserService, command.PetId, cancellationToken);

        var pet = await _dbContext.Set<Pet>()
            .Include(p => p.MedicalHistories)
            .Include(p => p.RehabPrograms)
            .FirstOrDefaultAsync(p => p.PetId == command.PetId, cancellationToken);

        if (pet is null)
        {
            throw new KeyNotFoundException($"Pet with ID {command.PetId} not found.");
        }

        var latestDiagnosis = pet.MedicalHistories.OrderByDescending(m => m.CreatedDate).FirstOrDefault()?.Diagnosis;
        var activeProgram = pet.RehabPrograms.FirstOrDefault(r => r.IsActive);

        var title = !string.IsNullOrWhiteSpace(command.CustomTitle)
            ? command.CustomTitle.Trim()
            : $"Clinical Progress Report - {DateTime.UtcNow:MMM dd, yyyy}";

        var summary = $"Comprehensive clinical rehabilitation summary for {pet.PetName}." +
            (latestDiagnosis != null ? $" Current diagnosis: {latestDiagnosis}." : "") +
            (activeProgram != null ? $" Active Plan: {activeProgram.ProgramTitle}." : "");

        var sharedReport = new SharedReport
        {
            PetId = command.PetId,
            SoapNoteId = null,
            SharedByPhysioId = _currentUserService.UserId!.Value,
            Title = title,
            ReportType = "CLINICAL_REPORT",
            Summary = summary,
            SharedAtUtc = DateTime.UtcNow
        };

        _dbContext.Set<SharedReport>().Add(sharedReport);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var created = await _dbContext.Set<SharedReport>()
            .Include(r => r.SharedByPhysio)
            .FirstAsync(r => r.SharedReportId == sharedReport.SharedReportId, cancellationToken);

        return SoapNotes.SoapNoteMapper.ToSharedReportDto(created);
    }
}
