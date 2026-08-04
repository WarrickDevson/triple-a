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
            .Include(p => p.RehabProgramExercises)
                .ThenInclude(e => e.Exercise)
                    .ThenInclude(ex => ex.Steps)
            .Where(p => p.PetId == query.PetId)
            .OrderByDescending(p => p.StartDate)
            .ToListAsync(cancellationToken);

        return programs.Select(RehabProgramMapper.ToDto).ToList();
    }
}
