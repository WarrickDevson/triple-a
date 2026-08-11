using KPW.Application.DTOs.Auth;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using KPW.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace KPW.Application.Features.Auth.Commands;

public record ResendVerificationEmailCommand(ResendVerificationEmailRequestDto Request) : IRequest<MessageResponseDto>;

public class ResendVerificationEmailCommandHandler : IRequestHandler<ResendVerificationEmailCommand, MessageResponseDto>
{
    private const string SuccessMessage = "If an unverified account exists for that email, a verification link has been sent.";

    private readonly DbContext _dbContext;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IEmailSender _emailSender;
    private readonly AppOptions _appOptions;

    public ResendVerificationEmailCommandHandler(
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

    public async Task<MessageResponseDto> Handle(ResendVerificationEmailCommand command, CancellationToken cancellationToken)
    {
        var email = command.Request.Email.Trim().ToLowerInvariant();
        var user = await _dbContext.Set<User>()
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        if (user is not null && !user.IsEmailVerified)
        {
            if (user.EmailVerificationTokenExpiresAt.HasValue &&
                user.EmailVerificationTokenExpiresAt.Value > DateTime.UtcNow.AddHours(23).AddMinutes(59))
            {
                throw new InvalidOperationException("Please wait 60 seconds before requesting another verification email.");
            }

            var rawToken = _jwtTokenService.GenerateRefreshToken();
            var tokenHash = _jwtTokenService.HashRefreshToken(rawToken);

            user.EmailVerificationTokenHash = tokenHash;
            user.EmailVerificationTokenExpiresAt = DateTime.UtcNow.AddHours(24);

            await _dbContext.SaveChangesAsync(cancellationToken);

            var baseUrl = user.UserRole == UserRole.Owner
                ? _appOptions.PublicOwnerAppUrl.TrimEnd('/')
                : _appOptions.PublicPortalUrl.TrimEnd('/');
            var verifyLink = $"{baseUrl}/verify-email?email={Uri.EscapeDataString(user.Email)}&token={Uri.EscapeDataString(rawToken)}";

            var body = $"""
                <div style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;">
                    <h2>Verify your MoveWell account</h2>
                    <p>Hello {user.FirstName},</p>
                    <p>Please click the link below to verify your email address.</p>
                    <p style="margin: 24px 0;">
                        <a href="{verifyLink}" style="background-color: #2563eb; color: #ffffff; padding: 12px 24px; text-decoration: none; border-radius: 6px; font-weight: bold;">Verify Email Address</a>
                    </p>
                    <p>Or copy and paste this URL into your web browser:</p>
                    <p><a href="{verifyLink}">{verifyLink}</a></p>
                    <p style="color: #6b7280; font-size: 14px; margin-top: 24px;">This verification link will expire in 24 hours.</p>
                </div>
                """;

            await _emailSender.SendAsync(
                user.Email,
                "Verify your MoveWell account",
                body,
                cancellationToken);
        }

        return new MessageResponseDto(SuccessMessage);
    }
}
