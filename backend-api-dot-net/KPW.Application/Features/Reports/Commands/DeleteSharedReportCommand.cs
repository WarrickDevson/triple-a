using KPW.Application.Features.Pets;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using KPW.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.Reports.Commands;

public record DeleteSharedReportCommand(int SharedReportId) : IRequest<bool>;

public class DeleteSharedReportCommandHandler : IRequestHandler<DeleteSharedReportCommand, bool>
{
    private readonly DbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public DeleteSharedReportCommandHandler(DbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(DeleteSharedReportCommand command, CancellationToken cancellationToken)
    {
        if (_currentUserService.Role is not (UserRole.Physio or UserRole.SysAdmin))
        {
            throw new UnauthorizedAccessException("Only physiotherapists can remove shared documents.");
        }

        var report = await _dbContext.Set<SharedReport>()
            .FirstOrDefaultAsync(r => r.SharedReportId == command.SharedReportId, cancellationToken);

        if (report is null)
        {
            return false;
        }

        await PetAuthorization.EnsureCanAccessPet(_dbContext, _currentUserService, report.PetId, cancellationToken);

        report.IsActive = false;

        // If it was linked to a soap note, also reflect unshared on the soap note
        if (report.SoapNoteId.HasValue)
        {
            var note = await _dbContext.Set<SoapNote>()
                .FirstOrDefaultAsync(s => s.SoapNoteId == report.SoapNoteId.Value, cancellationToken);
            if (note != null)
            {
                note.IsSharedWithOwner = false;
                note.SharedAtUtc = null;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
