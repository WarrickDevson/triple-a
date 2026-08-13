using KPW.Application.DTOs.SoapNotes;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.SoapNotes.Commands;

public record CreateOwnerSubjectiveNoteCommand(int PetId, CreateOwnerSubjectiveNoteRequestDto Request) : IRequest<OwnerSubjectiveNoteDto>;

public class CreateOwnerSubjectiveNoteCommandHandler : IRequestHandler<CreateOwnerSubjectiveNoteCommand, OwnerSubjectiveNoteDto>
{
    private readonly DbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public CreateOwnerSubjectiveNoteCommandHandler(DbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<OwnerSubjectiveNoteDto> Handle(CreateOwnerSubjectiveNoteCommand command, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var pet = await _dbContext.Set<Pet>()
            .FirstOrDefaultAsync(p => p.PetId == command.PetId, cancellationToken);

        if (pet is null)
        {
            throw new KeyNotFoundException($"Pet with ID {command.PetId} not found.");
        }

        var req = command.Request;
        var note = new OwnerSubjectiveNote
        {
            PetId = command.PetId,
            OwnerId = _currentUserService.UserId.Value,
            NoteDate = DateTime.UtcNow,
            Notes = req.Notes.Trim(),
            PainObserved = req.PainObserved,
            EnergyObserved = req.EnergyObserved,
            IsReviewed = false
        };

        _dbContext.Set<OwnerSubjectiveNote>().Add(note);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var created = await _dbContext.Set<OwnerSubjectiveNote>()
            .Include(n => n.Owner)
            .FirstAsync(n => n.OwnerSubjectiveNoteId == note.OwnerSubjectiveNoteId, cancellationToken);

        var ownerName = $"{created.Owner.FirstName} {created.Owner.LastName}".Trim();
        return new OwnerSubjectiveNoteDto(
            created.OwnerSubjectiveNoteId,
            created.PetId,
            created.OwnerId,
            ownerName,
            created.NoteDate,
            created.Notes,
            created.PainObserved,
            created.EnergyObserved,
            created.IsReviewed);
    }
}
