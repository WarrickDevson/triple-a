using KPW.Application.DTOs.RehabPrograms;
using KPW.Application.Features.Pets;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.RehabPrograms.Commands;

public record CompleteExerciseSessionCommand(
    int PetId,
    CompleteExerciseSessionRequestDto Request) : IRequest;

public class CompleteExerciseSessionCommandHandler : IRequestHandler<CompleteExerciseSessionCommand>
{
    private readonly DbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public CompleteExerciseSessionCommandHandler(DbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task Handle(CompleteExerciseSessionCommand command, CancellationToken cancellationToken)
    {
        await PetAuthorization.EnsureCanAccessPet(
            _dbContext, _currentUserService, command.PetId, cancellationToken);

        var request = command.Request;
        var programExists = await _dbContext.Set<RehabProgram>()
            .AnyAsync(
                p => p.RehabProgramId == request.RehabProgramId &&
                     p.PetId == command.PetId,
                cancellationToken);

        if (!programExists)
        {
            throw new KeyNotFoundException("Rehabilitation program not found for this pet.");
        }

        _dbContext.Set<ExerciseSessionLog>().Add(new ExerciseSessionLog
        {
            PetId = command.PetId,
            ExerciseId = request.ExerciseId,
            RehabProgramId = request.RehabProgramId,
            CompletedAt = DateTime.UtcNow
        });

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var logs = _dbContext.Set<DailyTrackingLog>();
        var existingLog = await logs
            .FirstOrDefaultAsync(l => l.PetId == command.PetId && l.LogDate == today, cancellationToken);

        if (existingLog is null)
        {
            logs.Add(new DailyTrackingLog
            {
                PetId = command.PetId,
                LogDate = today,
                IsCompleted = true
            });
        }
        else
        {
            existingLog.IsCompleted = true;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
