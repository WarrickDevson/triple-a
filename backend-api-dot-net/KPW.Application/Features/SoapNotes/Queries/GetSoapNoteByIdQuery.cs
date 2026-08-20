using KPW.Application.DTOs.SoapNotes;
using KPW.Application.Features.Pets;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using KPW.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.SoapNotes.Queries;

public record GetSoapNoteByIdQuery(int SoapNoteId) : IRequest<SoapNoteDto>;

public class GetSoapNoteByIdQueryHandler : IRequestHandler<GetSoapNoteByIdQuery, SoapNoteDto>
{
    private readonly DbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetSoapNoteByIdQueryHandler(DbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
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

        // Verify the caller has legitimate access to the pet
        await PetAuthorization.EnsureCanAccessPet(_dbContext, _currentUserService, note.PetId, cancellationToken);

        // If the user is an owner, verify the note is explicitly shared with them
        if (_currentUserService.Role == UserRole.Owner && !note.IsSharedWithOwner)
        {
            throw new UnauthorizedAccessException("This clinical assessment note has not been shared with you.");
        }

        return SoapNoteMapper.ToDto(note);
    }
}
