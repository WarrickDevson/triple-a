using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using KPW.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.SoapNotes.Commands;

public record DeleteOwnerSubjectiveNoteCommand(int OwnerSubjectiveNoteId) : IRequest<bool>;

public class DeleteOwnerSubjectiveNoteCommandHandler : IRequestHandler<DeleteOwnerSubjectiveNoteCommand, bool>
{
    private readonly DbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public DeleteOwnerSubjectiveNoteCommandHandler(DbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(DeleteOwnerSubjectiveNoteCommand command, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var note = await _dbContext.Set<OwnerSubjectiveNote>()
            .FirstOrDefaultAsync(n => n.OwnerSubjectiveNoteId == command.OwnerSubjectiveNoteId, cancellationToken);

        if (note is null)
        {
            throw new KeyNotFoundException($"Note with ID {command.OwnerSubjectiveNoteId} not found.");
        }

        var isOwner = note.OwnerId == _currentUserService.UserId.Value;
        var isPhysioOrAdmin = _currentUserService.Role is (UserRole.Physio or UserRole.SysAdmin);

        if (!isOwner && !isPhysioOrAdmin)
        {
            throw new UnauthorizedAccessException("You are not authorized to delete this note.");
        }

        note.IsActive = false;
        note.ModifiedDate = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
