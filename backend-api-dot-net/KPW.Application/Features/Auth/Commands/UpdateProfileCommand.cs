using KPW.Application.DTOs.Auth;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.Auth.Commands;

public record UpdateProfileCommand(UpdateProfileRequestDto Request) : IRequest<AuthUserDto>;

public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, AuthUserDto>
{
    private readonly DbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public UpdateProfileCommandHandler(
        DbContext dbContext,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<AuthUserDto> Handle(UpdateProfileCommand command, CancellationToken cancellationToken)
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

        user.FirstName = command.Request.FirstName.Trim();
        user.LastName = command.Request.LastName.Trim();
        if (command.Request.PhoneNumber != null)
        {
            user.PhoneNumber = command.Request.PhoneNumber.Trim();
        }

        Clinic? clinic = null;
        if (!string.IsNullOrWhiteSpace(command.Request.ClinicName))
        {
            var trimmedClinicName = command.Request.ClinicName.Trim();
            if (user.ClinicId is not null)
            {
                clinic = await _dbContext.Set<Clinic>()
                    .FirstOrDefaultAsync(c => c.ClinicId == user.ClinicId, cancellationToken);
                if (clinic is not null)
                {
                    clinic.ClinicName = trimmedClinicName;
                }
            }
            else
            {
                clinic = new Clinic
                {
                    ClinicName = trimmedClinicName,
                    InviteCode = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant(),
                    PhysicalAddress = string.Empty,
                    ContactNumber = string.Empty,
                };
                _dbContext.Set<Clinic>().Add(clinic);
                await _dbContext.SaveChangesAsync(cancellationToken);
                user.ClinicId = clinic.ClinicId;
            }
        }
        else if (user.ClinicId is not null)
        {
            clinic = await _dbContext.Set<Clinic>()
                .FirstOrDefaultAsync(c => c.ClinicId == user.ClinicId, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return AuthUserMapper.ToDto(user, clinic);
    }
}
