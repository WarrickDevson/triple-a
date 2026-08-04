using KPW.Application.DTOs.Auth;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.Auth.Commands;

public record ResetPasswordCommand(ResetPasswordRequestDto Request) : IRequest<MessageResponseDto>;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, MessageResponseDto>
{
    private readonly DbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public ResetPasswordCommandHandler(
        DbContext dbContext,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<MessageResponseDto> Handle(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        var tokenHash = _jwtTokenService.HashRefreshToken(command.Request.Token);
        var resetToken = await _dbContext.Set<PasswordResetToken>()
            .Include(t => t.User)
            .FirstOrDefaultAsync(
                t => t.TokenHash == tokenHash &&
                     t.UsedAt == null &&
                     t.ExpiresAt > DateTime.UtcNow,
                cancellationToken);

        if (resetToken is null)
        {
            throw new InvalidOperationException("Invalid or expired reset token.");
        }

        var user = resetToken.User;
        user.PasswordHash = _passwordHasher.HashPassword(command.Request.NewPassword);
        user.RefreshTokenHash = null;
        user.RefreshTokenExpiresAt = null;
        resetToken.UsedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new MessageResponseDto("Your password has been updated. You can sign in now.");
    }
}
