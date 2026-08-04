using KPW.Application.DTOs.Tracking;
using KPW.Application.Features.Pets;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.Tracking.Queries;

public record GetTrackingLogsQuery(int PetId, int Days = 14) : IRequest<IReadOnlyList<DailyTrackingLogDto>>;

public class GetTrackingLogsQueryHandler : IRequestHandler<GetTrackingLogsQuery, IReadOnlyList<DailyTrackingLogDto>>
{
    private readonly DbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetTrackingLogsQueryHandler(DbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<DailyTrackingLogDto>> Handle(
        GetTrackingLogsQuery query,
        CancellationToken cancellationToken)
    {
        await PetAuthorization.EnsureCanAccessPet(
            _dbContext, _currentUserService, query.PetId, cancellationToken);

        var fromDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-query.Days));

        var logs = await _dbContext.Set<DailyTrackingLog>()
            .Where(l => l.PetId == query.PetId && l.LogDate >= fromDate)
            .OrderByDescending(l => l.LogDate)
            .ToListAsync(cancellationToken);

        return logs.Select(l => new DailyTrackingLogDto(
            l.LogId,
            l.PetId,
            l.LogDate,
            l.PainScore,
            l.LamenessScore,
            l.EnergyScore,
            l.AppetiteScore,
            l.MobilityScore,
            l.WeightKg,
            l.IsCompleted)).ToList();
    }
}
