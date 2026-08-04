using KPW.Application.DTOs.RehabPrograms;
using KPW.Application.Features.Pets;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using KPW.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.RehabPrograms.Commands;

public record CreateRehabProgramCommand(CreateRehabProgramRequestDto Request) : IRequest<RehabProgramDto>;

public class CreateRehabProgramCommandHandler : IRequestHandler<CreateRehabProgramCommand, RehabProgramDto>
{
    private readonly DbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public CreateRehabProgramCommandHandler(DbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<RehabProgramDto> Handle(CreateRehabProgramCommand command, CancellationToken cancellationToken)
    {
        if (_currentUserService.Role is not (UserRole.Physio or UserRole.SysAdmin))
        {
            throw new UnauthorizedAccessException("Only physiotherapists can assign rehabilitation programs.");
        }

        var request = command.Request;
        await PetAuthorization.EnsureCanAccessPet(
            _dbContext, _currentUserService, request.PetId, cancellationToken);

        var exerciseIds = request.Exercises.Select(e => e.ExerciseId).Distinct().ToList();
        var existingExercises = await _dbContext.Set<Exercise>()
            .Where(e => exerciseIds.Contains(e.ExerciseId))
            .Select(e => e.ExerciseId)
            .ToListAsync(cancellationToken);

        if (existingExercises.Count != exerciseIds.Count)
        {
            throw new InvalidOperationException("One or more exercises were not found.");
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var overlappingPrograms = await _dbContext.Set<RehabProgram>()
                .Where(p => p.PetId == request.PetId)
                .ToListAsync(cancellationToken);

            foreach (var program in overlappingPrograms)
            {
                program.IsActive = false;
                program.EndDate = DateOnly.FromDateTime(DateTime.UtcNow);
            }

            var rehabProgram = new RehabProgram
            {
                PhysioId = _currentUserService.UserId!.Value,
                PetId = request.PetId,
                ProgramTitle = request.ProgramTitle.Trim(),
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Notes = request.Notes?.Trim()
            };

            _dbContext.Set<RehabProgram>().Add(rehabProgram);

            foreach (var exercise in request.Exercises)
            {
                _dbContext.Set<RehabProgramExercise>().Add(new RehabProgramExercise
                {
                    RehabProgram = rehabProgram,
                    ExerciseId = exercise.ExerciseId,
                    Repetitions = exercise.Repetitions,
                    Sets = exercise.Sets,
                    FrequencyPerDay = exercise.FrequencyPerDay
                });
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            var created = await _dbContext.Set<RehabProgram>()
                .Include(p => p.RehabProgramExercises)
                    .ThenInclude(e => e.Exercise)
                        .ThenInclude(ex => ex.Steps)
                .FirstAsync(p => p.RehabProgramId == rehabProgram.RehabProgramId, cancellationToken);

            return RehabProgramMapper.ToDto(created);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
