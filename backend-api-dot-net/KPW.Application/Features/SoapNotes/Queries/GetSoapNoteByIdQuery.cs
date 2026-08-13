using KPW.Application.DTOs.SoapNotes;
using KPW.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.SoapNotes.Queries;

public record GetSoapNoteByIdQuery(int SoapNoteId) : IRequest<SoapNoteDto>;

public class GetSoapNoteByIdQueryHandler : IRequestHandler<GetSoapNoteByIdQuery, SoapNoteDto>
{
    private readonly DbContext _dbContext;

    public GetSoapNoteByIdQueryHandler(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SoapNoteDto> Handle(GetSoapNoteByIdQuery query, CancellationToken cancellationToken)
    {
        var note = await _dbContext.Set<SoapNote>()
            .AsNoTracking()
            .Include(s => s.Physio)
            .FirstOrDefaultAsync(s => s.SoapNoteId == query.SoapNoteId, cancellationToken);

        if (note is null)
        {
            throw new KeyNotFoundException($"SOAP Note with ID {query.SoapNoteId} not found.");
        }

        return SoapNoteMapper.ToDto(note);
    }
}
