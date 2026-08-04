using KPW.Application.DTOs.Auth;
using KPW.Application.Features.Auth;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using KPW.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.Auth.Commands;

public record RegisterCommand(RegisterRequestDto Request) : IRequest<AuthResponseDto>;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponseDto>
{
    private readonly DbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public RegisterCommandHandler(
        DbContext dbContext,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponseDto> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var inviteCode = request.InviteCode.Trim().ToUpperInvariant();

        var clinic = await _dbContext.Set<Clinic>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.InviteCode == inviteCode, cancellationToken);

        if (clinic is null)
        {
            throw new InvalidOperationException("Invalid clinic invite code.");
        }

        var users = _dbContext.Set<User>();
        var email = request.Email.Trim().ToLowerInvariant();
        var emailExists = await users.AnyAsync(u => u.Email == email, cancellationToken);
        if (emailExists)
        {
            throw new InvalidOperationException("Email is already registered.");
        }

        var user = new User
        {
            Email = email,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            PhoneNumber = request.PhoneNumber?.Trim(),
            UserRole = UserRole.Owner,
            SubscriptionTier = SubscriptionTier.Free,
            ClinicId = clinic.ClinicId
        };

        users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await BuildAuthResponse(user, clinic, cancellationToken);
    }

    private async Task<AuthResponseDto> BuildAuthResponse(
        User user,
        Clinic? clinic,
        CancellationToken cancellationToken)
    {
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
