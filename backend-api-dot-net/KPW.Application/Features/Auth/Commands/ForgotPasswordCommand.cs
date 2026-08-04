using KPW.Application.DTOs.Auth;
using KPW.Application.Features.Auth;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using KPW.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace KPW.Application.Features.Auth.Commands;

public record ForgotPasswordCommand(ForgotPasswordRequestDto Request) : IRequest<ForgotPasswordResponseDto>;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, ForgotPasswordResponseDto>
{
    private const string SuccessMessage =
        "If an account exists for that email, we've sent password reset instructions.";

    private readonly DbContext _dbContext;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IEmailSender _emailSender;
    private readonly AppOptions _appOptions;

    public ForgotPasswordCommandHandler(
        DbContext dbContext,
        IJwtTokenService jwtTokenService,
        IEmailSender emailSender,
        IOptions<AppOptions> appOptions)
    {
        _dbContext = dbContext;
        _jwtTokenService = jwtTokenService;
        _emailSender = emailSender;
        _appOptions = appOptions.Value;
    }

    public async Task<ForgotPasswordResponseDto> Handle(
        ForgotPasswordCommand command,
        CancellationToken cancellationToken)
    {
        var email = command.Request.Email.Trim().ToLowerInvariant();
        var user = await _dbContext.Set<User>()
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        if (user is not null)
        {
            var rawToken = _jwtTokenService.GenerateRefreshToken();
            var tokenHash = _jwtTokenService.HashRefreshToken(rawToken);

            var existingTokens = await _dbContext.Set<PasswordResetToken>()
                .Where(t => t.UserId == user.UserId && t.UsedAt == null)
                .ToListAsync(cancellationToken);

            foreach (var existing in existingTokens)
            {
                existing.UsedAt = DateTime.UtcNow;
            }

            _dbContext.Set<PasswordResetToken>().Add(new PasswordResetToken
            {
                UserId = user.UserId,
                TokenHash = tokenHash,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                CreatedDate = DateTime.UtcNow
            });

            await _dbContext.SaveChangesAsync(cancellationToken);

            var baseUrl = user.UserRole == UserRole.Owner
                ? _appOptions.PublicOwnerAppUrl.TrimEnd('/')
                : _appOptions.PublicPortalUrl.TrimEnd('/');
            var resetLink = $"{baseUrl}/reset-password?token={Uri.EscapeDataString(rawToken)}";

            await _emailSender.SendAsync(
                user.Email,
                "Reset your KPW Companion password",
                $"Use this link to reset your password (expires in 1 hour): {resetLink}",
                cancellationToken);
        }

        return new ForgotPasswordResponseDto(SuccessMessage);
    }
}
