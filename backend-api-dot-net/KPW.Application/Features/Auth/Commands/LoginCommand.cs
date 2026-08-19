using KPW.Application.DTOs.Auth;
using KPW.Application.Features.Auth;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.Logging;

namespace KPW.Application.Features.Auth.Commands;

public record LoginCommand(LoginRequestDto Request) : IRequest<AuthResponseDto>;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly DbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        DbContext dbContext,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        ILogger<LoginCommandHandler> logger)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    public async Task<AuthResponseDto> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _dbContext.Set<User>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email, cancellationToken);

        if (user is null)
        {
            _logger.LogWarning("Login attempt failed: User not found for email {Email}", email);
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Login attempt blocked: Inactive account for user {Email}", email);
            throw new UnauthorizedAccessException("ACCOUNT_INACTIVE: This account is currently inactive. Please contact support.");
        }

        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login attempt failed: Password hash mismatch for user {Email}", email);
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        if (!user.IsEmailVerified)
        {
            _logger.LogWarning("Login attempt blocked: Unverified email for user {Email}", email);
            throw new UnauthorizedAccessException("EMAIL_NOT_VERIFIED: Please verify your email address before logging in. Check your inbox for the verification link.");
        }

        if (!user.IsApproved && user.UserRole == KPW.Domain.Enums.UserRole.Physio)
        {
            _logger.LogWarning("Login attempt blocked: Unapproved physio account for user {Email}", email);
            throw new UnauthorizedAccessException("PENDING_APPROVAL: Your email address is verified, but your Physio account is currently awaiting administrator approval.");
        }

        _logger.LogInformation("Login successful for user {Email} (UserId: {UserId})", email, user.UserId);

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
