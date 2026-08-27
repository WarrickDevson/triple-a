using KPW.Application.DTOs.SoapNotes;
using KPW.Application.Features.Pets;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using KPW.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.Reports.Queries;

public record GetRecentClinicReportsQuery(int? PetId = null) : IRequest<IReadOnlyList<SharedReportDto>>;

public class GetRecentClinicReportsQueryHandler : IRequestHandler<GetRecentClinicReportsQuery, IReadOnlyList<SharedReportDto>>
{
    private readonly DbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetRecentClinicReportsQueryHandler(DbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<SharedReportDto>> Handle(GetRecentClinicReportsQuery query, CancellationToken cancellationToken)
    {
        if (_currentUserService.Role is not (UserRole.Physio or UserRole.SysAdmin or UserRole.Owner))
        {
            throw new UnauthorizedAccessException("You are not authorized to view reports.");
        }

        var dbQuery = _dbContext.Set<SharedReport>()
            .AsNoTracking()
            .Include(r => r.SharedByPhysio)
            .Include(r => r.Pet)
                .ThenInclude(p => p.Owner)
            .AsQueryable();

        if (_currentUserService.Role == UserRole.Owner)
        {
            dbQuery = dbQuery.Where(r => r.IsActive);
        }

        if (query.PetId.HasValue)
        {
            await PetAuthorization.EnsureCanAccessPet(_dbContext, _currentUserService, query.PetId.Value, cancellationToken);
            dbQuery = dbQuery.Where(r => r.PetId == query.PetId.Value);
        }
        else if (_currentUserService.Role == UserRole.Owner)
        {
            dbQuery = dbQuery.Where(r => r.Pet.OwnerId == _currentUserService.UserId);
        }

        var reports = await dbQuery
            .OrderByDescending(r => r.SharedAtUtc)
            .Take(50)
            .ToListAsync(cancellationToken);

        return reports.Select(SoapNotes.SoapNoteMapper.ToSharedReportDto).ToList();
    }
}
