using KPW.Application.DTOs.Exercises;
using KPW.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.Exercises.Queries;

public record GetExercisesQuery(string? Species, string? Condition) : IRequest<IReadOnlyList<ExerciseDto>>;

public class GetExercisesQueryHandler : IRequestHandler<GetExercisesQuery, IReadOnlyList<ExerciseDto>>
{
    private readonly DbContext _dbContext;

    public GetExercisesQueryHandler(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ExerciseDto>> Handle(GetExercisesQuery query, CancellationToken cancellationToken)
    {
        var exercises = _dbContext.Set<Exercise>()
            .Include(e => e.Steps)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Species))
        {
            exercises = exercises.Where(e =>
                e.TargetSpecies == null || e.TargetSpecies == query.Species);
        }

        if (!string.IsNullOrWhiteSpace(query.Condition))
        {
            exercises = exercises.Where(e =>
                e.ConditionCategory == null || e.ConditionCategory == query.Condition);
        }

        var results = await exercises
            .OrderBy(e => e.Title)
            .ToListAsync(cancellationToken);

        return results.Select(ExerciseMapper.ToDto).ToList();
    }
}
