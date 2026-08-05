using KPW.Application.DTOs.Exercises;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using KPW.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.Exercises.Commands;

public record CreateExerciseCommand(CreateExerciseRequestDto Request) : IRequest<ExerciseDto>;

public class CreateExerciseCommandHandler : IRequestHandler<CreateExerciseCommand, ExerciseDto>
{
    private readonly DbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public CreateExerciseCommandHandler(DbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<ExerciseDto> Handle(CreateExerciseCommand command, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is null)
        {
            throw new UnauthorizedAccessException();
        }

        if (_currentUserService.Role is not (UserRole.Physio or UserRole.SysAdmin))
        {
            throw new UnauthorizedAccessException("Only physiotherapists can add new exercises.");
        }

        var request = command.Request;

        var exercise = new Exercise
        {
            Title = request.Title.Trim(),
            ShortDescription = request.ShortDescription?.Trim(),
            TargetedMuscles = request.TargetedMuscles?.Trim(),
            ClinicalPurpose = request.ClinicalPurpose?.Trim(),
            SafetyNotes = request.SafetyNotes?.Trim(),
            CommonMistakes = request.CommonMistakes?.Trim(),
            VideoUrl = request.VideoUrl?.Trim(),
            TargetSpecies = request.TargetSpecies?.Trim(),
            ConditionCategory = request.ConditionCategory?.Trim(),
            DifficultyLevel = Math.Clamp(request.DifficultyLevel, 1, 5)
        };

        if (request.Steps is not null && request.Steps.Count > 0)
        {
            var stepNumber = 1;
            foreach (var step in request.Steps)
            {
                if (string.IsNullOrWhiteSpace(step.StepInstruction)) continue;

                exercise.Steps.Add(new ExerciseStep
                {
                    StepNumber = step.StepNumber > 0 ? step.StepNumber : stepNumber++,
                    StepInstruction = step.StepInstruction.Trim(),
                    ImageUrl = step.ImageUrl?.Trim()
                });
            }
        }

        _dbContext.Set<Exercise>().Add(exercise);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var created = await _dbContext.Set<Exercise>()
            .Include(e => e.Steps)
            .FirstAsync(e => e.ExerciseId == exercise.ExerciseId, cancellationToken);

        return ExerciseMapper.ToDto(created);
    }
}
