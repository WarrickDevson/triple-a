using KPW.Application.DTOs.SoapNotes;
using KPW.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.SoapNotes.Queries;

public record GetOwnerSubjectiveNotesQuery(int PetId) : IRequest<IReadOnlyList<OwnerSubjectiveNoteDto>>;

public class GetOwnerSubjectiveNotesQueryHandler : IRequestHandler<GetOwnerSubjectiveNotesQuery, IReadOnlyList<OwnerSubjectiveNoteDto>>
{
    private readonly DbContext _dbContext;

    public GetOwnerSubjectiveNotesQueryHandler(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<OwnerSubjectiveNoteDto>> Handle(GetOwnerSubjectiveNotesQuery query, CancellationToken cancellationToken)
    {
        var notes = await _dbContext.Set<OwnerSubjectiveNote>()
            .AsNoTracking()
            .Include(n => n.Owner)
            .Where(n => n.PetId == query.PetId)
            .OrderByDescending(n => n.NoteDate)
            .ToListAsync(cancellationToken);

        return notes.Select(n => new OwnerSubjectiveNoteDto(
            n.OwnerSubjectiveNoteId,
            n.PetId,
            n.OwnerId,
            $"{n.Owner.FirstName} {n.Owner.LastName}".Trim(),
            n.NoteDate,
            n.Notes,
            n.PainObserved,
            n.EnergyObserved,
            n.IsReviewed)).ToList();
    }
}
