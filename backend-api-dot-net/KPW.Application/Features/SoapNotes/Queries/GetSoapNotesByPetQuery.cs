using KPW.Application.DTOs.SoapNotes;
using KPW.Application.Features.Pets;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using KPW.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.SoapNotes.Queries;

public record GetSoapNotesByPetQuery(int PetId) : IRequest<IReadOnlyList<SoapNoteDto>>;

public class GetSoapNotesByPetQueryHandler : IRequestHandler<GetSoapNotesByPetQuery, IReadOnlyList<SoapNoteDto>>
{
    private readonly DbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetSoapNotesByPetQueryHandler(DbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<SoapNoteDto>> Handle(GetSoapNotesByPetQuery query, CancellationToken cancellationToken)
    {
        await PetAuthorization.EnsureCanAccessPet(_dbContext, _currentUserService, query.PetId, cancellationToken);

        var queryable = _dbContext.Set<SoapNote>()
            .AsNoTracking()
            .Include(s => s.Physio)
            .Where(s => s.PetId == query.PetId && s.IsActive);

        // Owners only see notes that have been shared with them
        if (_currentUserService.Role == UserRole.Owner)
        {
            queryable = queryable.Where(s => s.IsSharedWithOwner);
        }

        var notes = await queryable
            .OrderByDescending(s => s.SessionDate)
            .ToListAsync(cancellationToken);

        return notes.Select(SoapNoteMapper.ToDto).ToList();
    }
}
