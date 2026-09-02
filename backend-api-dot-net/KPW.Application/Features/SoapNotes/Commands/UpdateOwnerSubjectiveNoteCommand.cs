using KPW.Application.DTOs.SoapNotes;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using KPW.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.SoapNotes.Commands;

public record UpdateOwnerSubjectiveNoteCommand(
    int OwnerSubjectiveNoteId,
    UpdateOwnerSubjectiveNoteRequestDto Request) : IRequest<OwnerSubjectiveNoteDto>;

public class UpdateOwnerSubjectiveNoteCommandHandler : IRequestHandler<UpdateOwnerSubjectiveNoteCommand, OwnerSubjectiveNoteDto>
{
    private readonly DbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public UpdateOwnerSubjectiveNoteCommandHandler(DbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<OwnerSubjectiveNoteDto> Handle(UpdateOwnerSubjectiveNoteCommand command, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var note = await _dbContext.Set<OwnerSubjectiveNote>()
            .Include(n => n.Owner)
            .FirstOrDefaultAsync(n => n.OwnerSubjectiveNoteId == command.OwnerSubjectiveNoteId, cancellationToken);

        if (note is null)
        {
            throw new KeyNotFoundException($"Note with ID {command.OwnerSubjectiveNoteId} not found.");
        }

        var isOwner = note.OwnerId == _currentUserService.UserId.Value;
        var isPhysioOrAdmin = _currentUserService.Role is (UserRole.Physio or UserRole.SysAdmin);

        if (!isOwner && !isPhysioOrAdmin)
        {
            throw new UnauthorizedAccessException("You are not authorized to edit this note.");
        }

        var req = command.Request;
        note.Notes = req.Notes.Trim();
        note.PainObserved = req.PainObserved;
        note.EnergyObserved = req.EnergyObserved;
        note.ModifiedDate = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        var ownerName = $"{note.Owner.FirstName} {note.Owner.LastName}".Trim();
        return new OwnerSubjectiveNoteDto(
            note.OwnerSubjectiveNoteId,
            note.PetId,
            note.OwnerId,
            ownerName,
            note.NoteDate,
            note.Notes,
            note.PainObserved,
            note.EnergyObserved,
            note.IsReviewed);
    }
}
