using KPW.Application.DTOs.SoapNotes;
using KPW.Application.Features.Pets;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using KPW.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.SoapNotes.Commands;

public record ToggleSoapNoteShareCommand(int SoapNoteId, bool ShareWithOwner) : IRequest<SoapNoteDto>;

public class ToggleSoapNoteShareCommandHandler : IRequestHandler<ToggleSoapNoteShareCommand, SoapNoteDto>
{
    private readonly DbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public ToggleSoapNoteShareCommandHandler(DbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<SoapNoteDto> Handle(ToggleSoapNoteShareCommand command, CancellationToken cancellationToken)
    {
        if (_currentUserService.Role is not (UserRole.Physio or UserRole.SysAdmin))
        {
            throw new UnauthorizedAccessException("Only physiotherapists can change document sharing settings.");
        }

        var note = await _dbContext.Set<SoapNote>()
            .Include(s => s.Physio)
            .FirstOrDefaultAsync(s => s.SoapNoteId == command.SoapNoteId, cancellationToken);

        if (note is null)
        {
            throw new KeyNotFoundException($"SOAP note with ID {command.SoapNoteId} not found.");
        }

        await PetAuthorization.EnsureCanAccessPet(_dbContext, _currentUserService, note.PetId, cancellationToken);

        if (command.ShareWithOwner && !note.IsSharedWithOwner)
        {
            note.IsSharedWithOwner = true;
            note.SharedAtUtc = DateTime.UtcNow;

            var sharedReport = new SharedReport
            {
                PetId = note.PetId,
                SoapNoteId = note.SoapNoteId,
                SharedByPhysioId = _currentUserService.UserId!.Value,
                Title = $"SOAP Session Report - {note.SessionDate:MMM dd, yyyy}",
                ReportType = "SOAP_SESSION",
                Summary = !string.IsNullOrWhiteSpace(note.Plan) ? note.Plan : note.Subjective,
                SharedAtUtc = DateTime.UtcNow
            };
            _dbContext.Set<SharedReport>().Add(sharedReport);
        }
        else if (!command.ShareWithOwner && note.IsSharedWithOwner)
        {
            note.IsSharedWithOwner = false;
            note.SharedAtUtc = null;

            var existingReports = await _dbContext.Set<SharedReport>()
                .Where(r => r.SoapNoteId == note.SoapNoteId)
                .ToListAsync(cancellationToken);
            _dbContext.Set<SharedReport>().RemoveRange(existingReports);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return SoapNoteMapper.ToDto(note);
    }
}
