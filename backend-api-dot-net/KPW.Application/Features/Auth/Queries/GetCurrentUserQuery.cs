using KPW.Application.DTOs.Auth;
using KPW.Application.Features.Auth;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.Auth.Queries;

public record GetCurrentUserQuery : IRequest<AuthUserDto>;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, AuthUserDto>
{
    private readonly DbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetCurrentUserQueryHandler(DbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<AuthUserDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is null)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var user = await _dbContext.Set<User>()
            .FirstOrDefaultAsync(u => u.UserId == _currentUserService.UserId, cancellationToken);

        if (user is null)
        {
            throw new UnauthorizedAccessException("User not found.");
        }

        var clinic = user.ClinicId is null
            ? null
            : await _dbContext.Set<Clinic>()
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.ClinicId == user.ClinicId, cancellationToken);

        return AuthUserMapper.ToDto(user, clinic);
    }
}
