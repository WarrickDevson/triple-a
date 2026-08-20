using KPW.Application.Features.Pets;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using KPW.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.SoapNotes.Commands;

public record DeleteSoapNoteCommand(int SoapNoteId) : IRequest<bool>;

public class DeleteSoapNoteCommandHandler : IRequestHandler<DeleteSoapNoteCommand, bool>
{
    private readonly DbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public DeleteSoapNoteCommandHandler(DbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(DeleteSoapNoteCommand command, CancellationToken cancellationToken)
    {
        if (_currentUserService.Role is not (UserRole.Physio or UserRole.SysAdmin))
        {
            throw new UnauthorizedAccessException("Only physiotherapists can delete assessment notes.");
        }

        var note = await _dbContext.Set<SoapNote>()
            .FirstOrDefaultAsync(s => s.SoapNoteId == command.SoapNoteId, cancellationToken);

        if (note is null)
        {
            return false;
        }

        await PetAuthorization.EnsureCanAccessPet(_dbContext, _currentUserService, note.PetId, cancellationToken);

        note.IsActive = false;

        var linkedReports = await _dbContext.Set<SharedReport>()
            .Where(r => r.SoapNoteId == command.SoapNoteId)
            .ToListAsync(cancellationToken);

        foreach (var lr in linkedReports)
        {
            lr.IsActive = false;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
