using KPW.Application.DTOs.Exercises;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using KPW.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.Exercises.Queries;

public record GetExercisesQuery(string? Species, string? Condition) : IRequest<IReadOnlyList<ExerciseDto>>;

public class GetExercisesQueryHandler : IRequestHandler<GetExercisesQuery, IReadOnlyList<ExerciseDto>>
{
    private readonly DbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetExercisesQueryHandler(DbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<ExerciseDto>> Handle(GetExercisesQuery query, CancellationToken cancellationToken)
    {
        int? userClinicId = null;
        var role = _currentUserService.Role;

        if (_currentUserService.UserId is not null && role is not UserRole.SysAdmin)
        {
            var user = await _dbContext.Set<User>()
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == _currentUserService.UserId, cancellationToken);
            userClinicId = user?.ClinicId;
        }

        var exercisesQuery = _dbContext.Set<Exercise>()
            .Include(e => e.Steps)
            .AsNoTracking()
            .AsQueryable();

        if (role == UserRole.SysAdmin)
        {
            // Admin manages all system defaults
            exercisesQuery = exercisesQuery.Where(e => e.IsSystemDefault || e.ClinicId == null);
        }
        else if (userClinicId.HasValue)
        {
            // Physio/Owner: system defaults OR this clinic's custom exercises
            exercisesQuery = exercisesQuery.Where(e => (e.IsSystemDefault || e.ClinicId == null) || e.ClinicId == userClinicId.Value);
        }
        else
        {
            // Fallback: only system defaults
            exercisesQuery = exercisesQuery.Where(e => e.IsSystemDefault || e.ClinicId == null);
        }

        if (!string.IsNullOrWhiteSpace(query.Species))
        {
            exercisesQuery = exercisesQuery.Where(e =>
                e.TargetSpecies == null || e.TargetSpecies == query.Species);
        }

        if (!string.IsNullOrWhiteSpace(query.Condition))
        {
            exercisesQuery = exercisesQuery.Where(e =>
                e.ConditionCategory == null || e.ConditionCategory == query.Condition);
        }

        var allFetched = await exercisesQuery
            .OrderBy(e => e.Title)
            .ToListAsync(cancellationToken);

        // Build mapping of custom overrides for this clinic
        var overridesByBaseId = allFetched
            .Where(e => e.BaseExerciseId.HasValue && e.ClinicId == userClinicId)
            .ToDictionary(e => e.BaseExerciseId!.Value, e => e);

        var dtoList = new List<ExerciseDto>();

        foreach (var ex in allFetched)
        {
            var isBase = ex.IsSystemDefault || ex.ClinicId == null;
            if (isBase && overridesByBaseId.TryGetValue(ex.ExerciseId, out var overrideEx))
            {
                // Base default exercise with linked clinic override
                dtoList.Add(ExerciseMapper.ToDto(
                    ex,
                    hasCustomOverride: true,
                    customExerciseId: overrideEx.ExerciseId,
                    isCustomActive: overrideEx.IsActiveForOwners));
            }
            else
            {
                dtoList.Add(ExerciseMapper.ToDto(ex));
            }
        }

        return dtoList;
    }
}

