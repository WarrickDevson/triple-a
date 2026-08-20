using KPW.Application.DTOs.SoapNotes;
using KPW.Application.Features.Pets;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using KPW.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.Reports.Commands;

public record ShareDocumentCommand(int PetId, ShareDocumentRequestDto Request) : IRequest<SharedReportDto>;

public class ShareDocumentCommandHandler : IRequestHandler<ShareDocumentCommand, SharedReportDto>
{
    private readonly DbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public ShareDocumentCommandHandler(DbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<SharedReportDto> Handle(ShareDocumentCommand command, CancellationToken cancellationToken)
    {
        if (_currentUserService.Role is not (UserRole.Physio or UserRole.SysAdmin))
        {
            throw new UnauthorizedAccessException("Only physiotherapists can share documents with owners.");
        }

        await PetAuthorization.EnsureCanAccessPet(_dbContext, _currentUserService, command.PetId, cancellationToken);

        var pet = await _dbContext.Set<Pet>()
            .FirstOrDefaultAsync(p => p.PetId == command.PetId, cancellationToken);

        if (pet is null)
        {
            throw new KeyNotFoundException($"Pet with ID {command.PetId} not found.");
        }

        var req = command.Request;
        var sharedReport = new SharedReport
        {
            PetId = command.PetId,
            SoapNoteId = req.SoapNoteId,
            SharedByPhysioId = _currentUserService.UserId!.Value,
            Title = !string.IsNullOrWhiteSpace(req.Title) ? req.Title.Trim() : "Clinical Document Record",
            ReportType = !string.IsNullOrWhiteSpace(req.ReportType) ? req.ReportType.Trim() : "CLINICAL_DOCUMENT",
            Summary = req.Summary?.Trim(),
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
