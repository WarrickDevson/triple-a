using KPW.Application.DTOs.SoapNotes;
using KPW.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.Reports.Queries;

public record GetSharedReportsByPetQuery(int PetId) : IRequest<IReadOnlyList<SharedReportDto>>;

public class GetSharedReportsByPetQueryHandler : IRequestHandler<GetSharedReportsByPetQuery, IReadOnlyList<SharedReportDto>>
{
    private readonly DbContext _dbContext;

    public GetSharedReportsByPetQueryHandler(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<SharedReportDto>> Handle(GetSharedReportsByPetQuery query, CancellationToken cancellationToken)
    {
        var reports = await _dbContext.Set<SharedReport>()
            .AsNoTracking()
            .Include(r => r.SharedByPhysio)
            .Where(r => r.PetId == query.PetId)
            .OrderByDescending(r => r.SharedAtUtc)
            .ToListAsync(cancellationToken);

        return reports.Select(Features.SoapNotes.SoapNoteMapper.ToSharedReportDto).ToList();
    }
}
