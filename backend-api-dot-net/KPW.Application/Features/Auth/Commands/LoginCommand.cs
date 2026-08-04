using KPW.Application.DTOs.Auth;
using KPW.Application.Features.Auth;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.Auth.Commands;

public record LoginCommand(LoginRequestDto Request) : IRequest<AuthResponseDto>;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly DbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginCommandHandler(
        DbContext dbContext,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponseDto> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var user = await _dbContext.Set<User>()
            .FirstOrDefaultAsync(u => u.Email == request.Email.Trim().ToLowerInvariant(), cancellationToken);

        if (user is null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var clinic = user.ClinicId is null
            ? null
            : await _dbContext.Set<Clinic>()
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.ClinicId == user.ClinicId, cancellationToken);

        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();
        user.RefreshTokenHash = _jwtTokenService.HashRefreshToken(refreshToken);
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto(
            accessToken,
            refreshToken,
            DateTime.UtcNow.AddHours(1),
            AuthUserMapper.ToDto(user, clinic));
    }
}
