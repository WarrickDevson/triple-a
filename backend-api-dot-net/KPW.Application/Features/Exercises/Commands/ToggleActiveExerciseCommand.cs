using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using KPW.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.Exercises.Commands;

public record ToggleActiveExerciseResult(int ExerciseId, int? BaseExerciseId, bool IsActiveForOwners);

public record ToggleActiveExerciseCommand(int ExerciseId) : IRequest<ToggleActiveExerciseResult>;

public class ToggleActiveExerciseCommandHandler : IRequestHandler<ToggleActiveExerciseCommand, ToggleActiveExerciseResult>
{
    private readonly DbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public ToggleActiveExerciseCommandHandler(DbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<ToggleActiveExerciseResult> Handle(ToggleActiveExerciseCommand command, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is null)
        {
            throw new UnauthorizedAccessException();
        }

        if (_currentUserService.Role is not (UserRole.Physio or UserRole.SysAdmin))
        {
            throw new UnauthorizedAccessException("Only physiotherapists can toggle active exercise versions.");
        }

        var user = await _dbContext.Set<User>()
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == _currentUserService.UserId, cancellationToken);

        if (user?.ClinicId is null)
        {
            throw new InvalidOperationException("You must belong to a clinic to manage exercise active versions.");
        }

        var clinicId = user.ClinicId.Value;

        // Try to find the custom override by exercise ID or by base exercise ID
        var customExercise = await _dbContext.Set<Exercise>()
            .FirstOrDefaultAsync(e =>
                (e.ExerciseId == command.ExerciseId && e.ClinicId == clinicId) ||
                (e.BaseExerciseId == command.ExerciseId && e.ClinicId == clinicId), cancellationToken);

        if (customExercise is null)
        {
            throw new KeyNotFoundException($"No custom override found for exercise {command.ExerciseId} in your clinic.");
        }

        customExercise.IsActiveForOwners = !customExercise.IsActiveForOwners;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ToggleActiveExerciseResult(customExercise.ExerciseId, customExercise.BaseExerciseId, customExercise.IsActiveForOwners);
    }
}
