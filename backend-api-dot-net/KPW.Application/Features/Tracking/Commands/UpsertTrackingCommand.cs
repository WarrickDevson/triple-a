using KPW.Application.DTOs.Tracking;
using KPW.Application.Features.Pets;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.Tracking.Commands;

public record UpsertTrackingCommand(int PetId, UpsertTrackingRequestDto Request) : IRequest<DailyTrackingLogDto>;

public class UpsertTrackingCommandHandler : IRequestHandler<UpsertTrackingCommand, DailyTrackingLogDto>
{
    private readonly DbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public UpsertTrackingCommandHandler(DbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<DailyTrackingLogDto> Handle(UpsertTrackingCommand command, CancellationToken cancellationToken)
    {
        await PetAuthorization.EnsureCanAccessPet(
            _dbContext, _currentUserService, command.PetId, cancellationToken);

        ValidateScore(command.Request.PainScore, nameof(command.Request.PainScore));
        ValidateScore(command.Request.EnergyScore, nameof(command.Request.EnergyScore));
        ValidateScore(command.Request.MobilityScore, nameof(command.Request.MobilityScore));
        ValidateScore(command.Request.AppetiteScore, nameof(command.Request.AppetiteScore));
        ValidateScore(command.Request.LamenessScore, nameof(command.Request.LamenessScore));

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var logs = _dbContext.Set<DailyTrackingLog>();

        var existing = await logs
            .FirstOrDefaultAsync(l => l.PetId == command.PetId && l.LogDate == today, cancellationToken);

        if (existing is null)
        {
            existing = new DailyTrackingLog
            {
                PetId = command.PetId,
                LogDate = today,
                IsCompleted = true
            };
            logs.Add(existing);
        }

        existing.PainScore = command.Request.PainScore;
        existing.EnergyScore = command.Request.EnergyScore;
        existing.MobilityScore = command.Request.MobilityScore;
        existing.AppetiteScore = command.Request.AppetiteScore;
        existing.LamenessScore = command.Request.LamenessScore;
        existing.WeightKg = command.Request.WeightKg;
        existing.IsCompleted = true;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToDto(existing);
    }

    private static void ValidateScore(int? score, string fieldName)
    {
        if (score is null) return;
        if (score is < 1 or > 10)
        {
            throw new InvalidOperationException($"{fieldName} must be between 1 and 10.");
        }
    }

    private static DailyTrackingLogDto ToDto(DailyTrackingLog log) =>
        new(
            log.LogId,
            log.PetId,
            log.LogDate,
            log.PainScore,
            log.LamenessScore,
            log.EnergyScore,
            log.AppetiteScore,
            log.MobilityScore,
            log.WeightKg,
            log.IsCompleted);
}
