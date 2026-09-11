using KPW.Application.DTOs.Exercises;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using KPW.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.Exercises.Commands;

public record UpdateExerciseCommand(int ExerciseId, UpdateExerciseRequestDto Request) : IRequest<ExerciseDto>;

public class UpdateExerciseCommandHandler : IRequestHandler<UpdateExerciseCommand, ExerciseDto>
{
    private readonly DbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public UpdateExerciseCommandHandler(DbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<ExerciseDto> Handle(UpdateExerciseCommand command, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is null)
        {
            throw new UnauthorizedAccessException();
        }

        var role = _currentUserService.Role;
        if (role is not (UserRole.Physio or UserRole.SysAdmin))
        {
            throw new UnauthorizedAccessException("Only physiotherapists and administrators can edit exercises.");
        }

        var exercise = await _dbContext.Set<Exercise>()
            .Include(e => e.Steps)
            .FirstOrDefaultAsync(e => e.ExerciseId == command.ExerciseId, cancellationToken);

        if (exercise is null)
        {
            throw new KeyNotFoundException($"Exercise {command.ExerciseId} not found.");
        }

        if (role == UserRole.SysAdmin)
        {
            // SysAdmin can edit system defaults
            if (!exercise.IsSystemDefault && exercise.ClinicId.HasValue)
            {
                throw new UnauthorizedAccessException("Administrators manage system default exercises.");
            }
        }
        else
        {
            // Physio can only edit exercises belonging to their clinic
            var user = await _dbContext.Set<User>()
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == _currentUserService.UserId, cancellationToken);

            if (user?.ClinicId is null || exercise.ClinicId != user.ClinicId)
            {
                throw new UnauthorizedAccessException("You can only edit custom exercises belonging to your clinic. Please customize this exercise first to create your own clinic version.");
            }
        }

        var request = command.Request;
        exercise.Title = request.Title.Trim();
        exercise.ShortDescription = request.ShortDescription?.Trim();
        exercise.TargetedMuscles = request.TargetedMuscles?.Trim();
        exercise.ClinicalPurpose = request.ClinicalPurpose?.Trim();
        exercise.SafetyNotes = request.SafetyNotes?.Trim();
        exercise.CommonMistakes = request.CommonMistakes?.Trim();
        exercise.VideoUrl = request.VideoUrl?.Trim();
        exercise.CoverImageUrl = request.CoverImageUrl?.Trim();
        exercise.TargetSpecies = request.TargetSpecies?.Trim();
        exercise.ConditionCategory = request.ConditionCategory?.Trim();
        exercise.DifficultyLevel = Math.Clamp(request.DifficultyLevel, 1, 5);

        // Replace or update steps
        _dbContext.Set<ExerciseStep>().RemoveRange(exercise.Steps);
        exercise.Steps.Clear();

        if (request.Steps is not null && request.Steps.Count > 0)
        {
            var stepNum = 1;
            foreach (var step in request.Steps)
            {
                if (string.IsNullOrWhiteSpace(step.StepInstruction)) continue;

                exercise.Steps.Add(new ExerciseStep
                {
                    ExerciseId = exercise.ExerciseId,
                    StepNumber = step.StepNumber > 0 ? step.StepNumber : stepNum++,
                    StepInstruction = step.StepInstruction.Trim(),
                    ImageUrl = step.ImageUrl?.Trim()
                });
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var updated = await _dbContext.Set<Exercise>()
            .Include(e => e.Steps)
            .FirstAsync(e => e.ExerciseId == exercise.ExerciseId, cancellationToken);

        return ExerciseMapper.ToDto(updated);
    }
}
