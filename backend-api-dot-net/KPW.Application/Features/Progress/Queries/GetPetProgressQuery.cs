using KPW.Application.DTOs.Progress;
using KPW.Application.Features.Pets;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.Progress.Queries;

public record GetPetProgressQuery(int PetId) : IRequest<PetProgressSummaryDto>;

public class GetPetProgressQueryHandler : IRequestHandler<GetPetProgressQuery, PetProgressSummaryDto>
{
    private readonly DbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetPetProgressQueryHandler(DbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<PetProgressSummaryDto> Handle(GetPetProgressQuery query, CancellationToken cancellationToken)
    {
        await PetAuthorization.EnsureCanAccessPet(
            _dbContext, _currentUserService, query.PetId, cancellationToken);

        var pet = await _dbContext.Set<Pet>()
            .AsNoTracking()
            .FirstAsync(p => p.PetId == query.PetId, cancellationToken);

        var logs = await _dbContext.Set<DailyTrackingLog>()
            .AsNoTracking()
            .Where(l => l.PetId == query.PetId)
            .OrderBy(l => l.LogDate)
            .Select(l => new PetProgressLogDto(
                l.LogDate,
                l.PainScore,
                l.LamenessScore,
                l.EnergyScore,
                l.AppetiteScore,
                l.MobilityScore,
                l.WeightKg,
                l.IsCompleted))
            .ToListAsync(cancellationToken);

        return new PetProgressSummaryDto(
            pet.PetId,
            pet.PetName,
            logs.Count(l => l.IsCompleted),
            logs.Count,
            logs);
    }
}
