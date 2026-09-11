using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using KPW.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.Exercises.Commands;

public record DeleteExerciseCommand(int ExerciseId) : IRequest<bool>;

public class DeleteExerciseCommandHandler : IRequestHandler<DeleteExerciseCommand, bool>
{
    private readonly DbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public DeleteExerciseCommandHandler(DbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(DeleteExerciseCommand command, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is null)
        {
            throw new UnauthorizedAccessException();
        }

        var role = _currentUserService.Role;
        if (role is not (UserRole.Physio or UserRole.SysAdmin))
        {
            throw new UnauthorizedAccessException();
        }

        var exercise = await _dbContext.Set<Exercise>()
            .FirstOrDefaultAsync(e => e.ExerciseId == command.ExerciseId, cancellationToken);

        if (exercise is null)
        {
            return false;
        }

        if (role != UserRole.SysAdmin)
        {
            var user = await _dbContext.Set<User>()
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == _currentUserService.UserId, cancellationToken);

            if (user?.ClinicId is null || exercise.ClinicId != user.ClinicId)
            {
                throw new UnauthorizedAccessException("You can only delete custom exercises belonging to your clinic.");
            }
        }

        exercise.IsActive = false;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
