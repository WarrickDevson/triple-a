using KPW.Application.DTOs.SoapNotes;
using KPW.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.SoapNotes.Queries;

public record GetSoapNotesByPetQuery(int PetId) : IRequest<IReadOnlyList<SoapNoteDto>>;

public class GetSoapNotesByPetQueryHandler : IRequestHandler<GetSoapNotesByPetQuery, IReadOnlyList<SoapNoteDto>>
{
    private readonly DbContext _dbContext;

    public GetSoapNotesByPetQueryHandler(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<SoapNoteDto>> Handle(GetSoapNotesByPetQuery query, CancellationToken cancellationToken)
    {
        var notes = await _dbContext.Set<SoapNote>()
            .AsNoTracking()
            .Include(s => s.Physio)
            .Where(s => s.PetId == query.PetId)
            .OrderByDescending(s => s.SessionDate)
            .ToListAsync(cancellationToken);

        return notes.Select(SoapNoteMapper.ToDto).ToList();
    }
}
