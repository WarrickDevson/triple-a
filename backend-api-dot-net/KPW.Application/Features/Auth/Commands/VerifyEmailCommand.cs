using KPW.Application.DTOs.Auth;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.Auth.Commands;

public record VerifyEmailCommand(VerifyEmailRequestDto Request) : IRequest<VerifyEmailResponseDto>;

public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, VerifyEmailResponseDto>
{
    private readonly DbContext _dbContext;
    private readonly IJwtTokenService _jwtTokenService;

    public VerifyEmailCommandHandler(
        DbContext dbContext,
        IJwtTokenService jwtTokenService)
    {
        _dbContext = dbContext;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<VerifyEmailResponseDto> Handle(VerifyEmailCommand command, CancellationToken cancellationToken)
    {
        var email = command.Request.Email.Trim().ToLowerInvariant();
        var rawToken = command.Request.Token.Trim();

        var user = await _dbContext.Set<User>()
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        if (user is null)
        {
            throw new InvalidOperationException("Invalid email or verification token.");
        }

        if (user.IsEmailVerified)
        {
            return new VerifyEmailResponseDto(
                "Email is already verified.",
                true,
                user.IsApproved,
                user.UserRole);
        }

        if (string.IsNullOrWhiteSpace(user.EmailVerificationTokenHash) ||
            !user.EmailVerificationTokenExpiresAt.HasValue ||
            user.EmailVerificationTokenExpiresAt < DateTime.UtcNow)
        {
            throw new InvalidOperationException("Verification token has expired or is invalid. Please request a new verification email.");
        }

        var inputTokenHash = _jwtTokenService.HashRefreshToken(rawToken);
        if (user.EmailVerificationTokenHash != inputTokenHash)
        {
            throw new InvalidOperationException("Invalid verification token.");
        }

        user.IsEmailVerified = true;
        user.EmailVerificationTokenHash = null;
        user.EmailVerificationTokenExpiresAt = null;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new VerifyEmailResponseDto(
            "Email verified successfully.",
            true,
            user.IsApproved,
            user.UserRole);
    }
}
