using KPW.Application.DTOs.RehabPrograms;
using KPW.Application.Features.Pets;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using KPW.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.RehabPrograms.Queries;

public record GetRehabProgramsByPetQuery(int PetId) : IRequest<IReadOnlyList<RehabProgramDto>>;

public class GetRehabProgramsByPetQueryHandler : IRequestHandler<GetRehabProgramsByPetQuery, IReadOnlyList<RehabProgramDto>>
{
    private readonly DbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetRehabProgramsByPetQueryHandler(DbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<RehabProgramDto>> Handle(
        GetRehabProgramsByPetQuery query,
        CancellationToken cancellationToken)
    {
        await PetAuthorization.EnsureCanAccessPet(
            _dbContext, _currentUserService, query.PetId, cancellationToken);

        var programs = await _dbContext.Set<RehabProgram>()
            .Include(p => p.Pet)
                .ThenInclude(pet => pet.Owner)
            .Include(p => p.RehabProgramExercises)
                .ThenInclude(e => e.Exercise)
                    .ThenInclude(ex => ex.Steps)
            .Where(p => p.PetId == query.PetId)
            .OrderByDescending(p => p.StartDate)
            .ToListAsync(cancellationToken);

        if (programs.Count > 0)
        {
            var petClinicId = programs[0].Pet?.Owner?.ClinicId;
            if (petClinicId.HasValue)
            {
                // Find all base exercise IDs used in these programs
                var baseExerciseIds = programs
                    .SelectMany(p => p.RehabProgramExercises)
                    .Select(re => re.Exercise.ExerciseId)
                    .Distinct()
                    .ToList();

                // Check for active clinic overrides for these base exercises
                var activeOverrides = await _dbContext.Set<Exercise>()
                    .Include(ex => ex.Steps)
                    .Where(ex => ex.ClinicId == petClinicId.Value &&
                                 ex.BaseExerciseId.HasValue &&
                                 baseExerciseIds.Contains(ex.BaseExerciseId.Value) &&
                                 ex.IsActiveForOwners)
                    .ToDictionaryAsync(ex => ex.BaseExerciseId!.Value, cancellationToken);

                foreach (var program in programs)
                {
                    foreach (var re in program.RehabProgramExercises)
                    {
                        if (activeOverrides.TryGetValue(re.ExerciseId, out var customOverride))
                        {
                            re.Exercise = customOverride;
                        }
                    }
                }
            }
        }

        return programs.Select(RehabProgramMapper.ToDto).ToList();
    }
}
