using KPW.Application.DTOs.Exercises;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using KPW.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.Exercises.Commands;

public record CustomizeExerciseCommand(int ExerciseId) : IRequest<ExerciseDto>;

public class CustomizeExerciseCommandHandler : IRequestHandler<CustomizeExerciseCommand, ExerciseDto>
{
    private readonly DbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public CustomizeExerciseCommandHandler(DbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<ExerciseDto> Handle(CustomizeExerciseCommand command, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is null)
        {
            throw new UnauthorizedAccessException();
        }

        if (_currentUserService.Role is not (UserRole.Physio or UserRole.SysAdmin))
        {
            throw new UnauthorizedAccessException("Only physiotherapists can customize exercises.");
        }

        var user = await _dbContext.Set<User>()
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == _currentUserService.UserId, cancellationToken);

        if (user?.ClinicId is null)
        {
            throw new InvalidOperationException("You must belong to a clinic to customize exercises.");
        }

        var clinicId = user.ClinicId.Value;

        // Check if a custom clinic version already exists for this base exercise
        var existingOverride = await _dbContext.Set<Exercise>()
            .Include(e => e.Steps)
            .FirstOrDefaultAsync(e => e.BaseExerciseId == command.ExerciseId && e.ClinicId == clinicId, cancellationToken);

        if (existingOverride is not null)
        {
            return ExerciseMapper.ToDto(existingOverride, hasCustomOverride: true, customExerciseId: existingOverride.ExerciseId, isCustomActive: existingOverride.IsActiveForOwners);
        }

        // Fetch original base exercise
        var baseExercise = await _dbContext.Set<Exercise>()
            .Include(e => e.Steps)
            .FirstOrDefaultAsync(e => e.ExerciseId == command.ExerciseId, cancellationToken);

        if (baseExercise is null)
        {
            throw new KeyNotFoundException($"Base exercise {command.ExerciseId} not found.");
        }

        // Clone base into a clinic custom override
        var customExercise = new Exercise
        {
            Title = baseExercise.Title,
            ShortDescription = baseExercise.ShortDescription,
            TargetedMuscles = baseExercise.TargetedMuscles,
            ClinicalPurpose = baseExercise.ClinicalPurpose,
            SafetyNotes = baseExercise.SafetyNotes,
            CommonMistakes = baseExercise.CommonMistakes,
            VideoUrl = baseExercise.VideoUrl,
            CoverImageUrl = baseExercise.CoverImageUrl,
            TargetSpecies = baseExercise.TargetSpecies,
            ConditionCategory = baseExercise.ConditionCategory,
            DifficultyLevel = baseExercise.DifficultyLevel,
            IsSystemDefault = false,
            ClinicId = clinicId,
            BaseExerciseId = baseExercise.ExerciseId,
            IsActiveForOwners = true
        };

        foreach (var step in baseExercise.Steps.OrderBy(s => s.StepNumber))
        {
            customExercise.Steps.Add(new ExerciseStep
            {
                StepNumber = step.StepNumber,
                StepInstruction = step.StepInstruction,
                ImageUrl = step.ImageUrl
            });
        }

        _dbContext.Set<Exercise>().Add(customExercise);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var created = await _dbContext.Set<Exercise>()
            .Include(e => e.Steps)
            .FirstAsync(e => e.ExerciseId == customExercise.ExerciseId, cancellationToken);

        return ExerciseMapper.ToDto(created, hasCustomOverride: true, customExerciseId: created.ExerciseId, isCustomActive: created.IsActiveForOwners);
    }
}
