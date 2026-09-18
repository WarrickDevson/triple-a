using KPW.Application.DTOs.Exercises;
using KPW.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.Exercises.Queries;

public record GetExerciseByIdQuery(int ExerciseId) : IRequest<ExerciseDto?>;

public class GetExerciseByIdQueryHandler : IRequestHandler<GetExerciseByIdQuery, ExerciseDto?>
{
    private readonly DbContext _dbContext;

    public GetExerciseByIdQueryHandler(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ExerciseDto?> Handle(GetExerciseByIdQuery query, CancellationToken cancellationToken)
    {
        var exercise = await _dbContext.Set<Exercise>()
            .Include(e => e.Steps)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.ExerciseId == query.ExerciseId, cancellationToken);

        return exercise is null ? null : ExerciseMapper.ToDto(exercise);
    }
}
