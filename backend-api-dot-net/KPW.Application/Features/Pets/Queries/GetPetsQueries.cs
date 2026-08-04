using KPW.Application.DTOs.Pets;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using KPW.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.Pets.Queries;

public record GetPetsByOwnerQuery(int OwnerId) : IRequest<IReadOnlyList<PetDto>>;

public class GetPetsByOwnerQueryHandler : IRequestHandler<GetPetsByOwnerQuery, IReadOnlyList<PetDto>>
{
    private readonly DbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetPetsByOwnerQueryHandler(DbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<PetDto>> Handle(GetPetsByOwnerQuery query, CancellationToken cancellationToken)
    {
        PetAuthorization.EnsureCanAccessOwner(_currentUserService, query.OwnerId);

        var pets = await _dbContext.Set<Pet>()
            .Include(p => p.Owner)
            .Include(p => p.MedicalHistories)
            .Where(p => p.OwnerId == query.OwnerId)
            .OrderBy(p => p.PetName)
            .ToListAsync(cancellationToken);

        return pets.Select(PetMapper.ToDto).ToList();
    }
}

public record GetClinicPatientsQuery : IRequest<IReadOnlyList<PetDto>>;

public class GetClinicPatientsQueryHandler : IRequestHandler<GetClinicPatientsQuery, IReadOnlyList<PetDto>>
{
    private readonly DbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetClinicPatientsQueryHandler(DbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<PetDto>> Handle(GetClinicPatientsQuery request, CancellationToken cancellationToken)
    {
        if (_currentUserService.Role is not (UserRole.Physio or UserRole.SysAdmin))
        {
            throw new UnauthorizedAccessException();
        }

        var currentUser = await _dbContext.Set<User>()
            .AsNoTracking()
            .FirstAsync(u => u.UserId == _currentUserService.UserId, cancellationToken);

        if (currentUser.ClinicId is null && _currentUserService.Role != UserRole.SysAdmin)
        {
            return [];
        }

        var query = _dbContext.Set<Pet>()
            .Include(p => p.Owner)
            .Include(p => p.MedicalHistories)
            .AsQueryable();

        if (currentUser.ClinicId is not null)
        {
            query = query.Where(p => p.Owner.ClinicId == currentUser.ClinicId);
        }

        var pets = await query.OrderBy(p => p.PetName).ToListAsync(cancellationToken);
        return pets.Select(PetMapper.ToDto).ToList();
    }
}
