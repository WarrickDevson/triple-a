using KPW.Application.DTOs.Auth;
using KPW.Application.Features.Auth;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.Auth.Commands;

public record RefreshTokenCommand(RefreshTokenRequestDto Request) : IRequest<AuthResponseDto>;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponseDto>
{
    private readonly DbContext _dbContext;
    private readonly IJwtTokenService _jwtTokenService;

    public RefreshTokenCommandHandler(DbContext dbContext, IJwtTokenService jwtTokenService)
    {
        _dbContext = dbContext;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponseDto> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var refreshTokenHash = _jwtTokenService.HashRefreshToken(command.Request.RefreshToken);
        var user = await _dbContext.Set<User>()
            .FirstOrDefaultAsync(
                u => u.RefreshTokenHash == refreshTokenHash &&
                     u.RefreshTokenExpiresAt > DateTime.UtcNow,
                cancellationToken);

        if (user is null)
        {
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");
        }

        var clinic = user.ClinicId is null
            ? null
            : await _dbContext.Set<Clinic>()
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.ClinicId == user.ClinicId, cancellationToken);

        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var newRefreshToken = _jwtTokenService.GenerateRefreshToken();
        user.RefreshTokenHash = _jwtTokenService.HashRefreshToken(newRefreshToken);
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto(
            accessToken,
            newRefreshToken,
            DateTime.UtcNow.AddHours(1),
            AuthUserMapper.ToDto(user, clinic));
    }
}
