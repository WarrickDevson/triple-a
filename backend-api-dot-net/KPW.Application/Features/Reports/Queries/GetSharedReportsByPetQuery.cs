using KPW.Application.DTOs.SoapNotes;
using KPW.Application.Features.Pets;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.Reports.Queries;

public record GetSharedReportsByPetQuery(int PetId) : IRequest<IReadOnlyList<SharedReportDto>>;

public class GetSharedReportsByPetQueryHandler : IRequestHandler<GetSharedReportsByPetQuery, IReadOnlyList<SharedReportDto>>
{
    private readonly DbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetSharedReportsByPetQueryHandler(DbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<SharedReportDto>> Handle(GetSharedReportsByPetQuery query, CancellationToken cancellationToken)
    {
        await PetAuthorization.EnsureCanAccessPet(_dbContext, _currentUserService, query.PetId, cancellationToken);

        var reports = await _dbContext.Set<SharedReport>()
            .AsNoTracking()
            .Include(r => r.SharedByPhysio)
            .Where(r => r.PetId == query.PetId && r.IsActive)
            .OrderByDescending(r => r.SharedAtUtc)
            .ToListAsync(cancellationToken);

        return reports.Select(Features.SoapNotes.SoapNoteMapper.ToSharedReportDto).ToList();
    }
}
