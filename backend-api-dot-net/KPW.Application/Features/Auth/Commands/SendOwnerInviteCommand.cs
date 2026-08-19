using KPW.Application.DTOs.Auth;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace KPW.Application.Features.Auth.Commands;

public record SendOwnerInviteCommand(SendOwnerInviteRequestDto Request) : IRequest<MessageResponseDto>;

public class SendOwnerInviteCommandHandler : IRequestHandler<SendOwnerInviteCommand, MessageResponseDto>
{
    private readonly DbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEmailSender _emailSender;
    private readonly AppOptions _appOptions;

    public SendOwnerInviteCommandHandler(
        DbContext dbContext,
        ICurrentUserService currentUserService,
        IEmailSender emailSender,
        IOptions<AppOptions> appOptions)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _emailSender = emailSender;
        _appOptions = appOptions.Value;
    }

    public async Task<MessageResponseDto> Handle(SendOwnerInviteCommand command, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        if (!currentUserId.HasValue || currentUserId.Value == 0)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var physioUser = await _dbContext.Set<User>()
            .Include(u => u.Clinic)
            .FirstOrDefaultAsync(u => u.UserId == currentUserId.Value, cancellationToken);

        if (physioUser is null || physioUser.Clinic is null || string.IsNullOrWhiteSpace(physioUser.Clinic.InviteCode))
        {
            throw new InvalidOperationException("Your account is not associated with a clinic containing a valid invite code.");
        }

        var clinic = physioUser.Clinic;
        var recipientEmail = command.Request.RecipientEmail.Trim().ToLowerInvariant();
        var recipientGreeting = !string.IsNullOrWhiteSpace(command.Request.OwnerName)
            ? command.Request.OwnerName.Trim()
            : "Pet Owner";

        var ownerAppUrl = _appOptions.PublicOwnerAppUrl.TrimEnd('/');
        var registerLink = $"{ownerAppUrl}/register?inviteCode={Uri.EscapeDataString(clinic.InviteCode)}";

        var subject = $"Invitation to join Triple A from {clinic.ClinicName}";
        var body = $"""
            <div style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; color: #1e293b;">
                <h2 style="color: #1e3a8a;">Welcome to Triple A!</h2>
                <p>Hello {recipientGreeting},</p>
                <p><strong>{physioUser.FirstName} {physioUser.LastName}</strong> from <strong>{clinic.ClinicName}</strong> has invited you to create your Triple A Companion account to manage your pet's rehabilitation and home exercise plans.</p>

                <div style="background-color: #f1f5f9; border-left: 4px solid #2563eb; padding: 16px; margin: 20px 0; border-radius: 4px;">
                    <p style="margin: 0; font-size: 14px; color: #64748b; text-transform: uppercase; font-weight: bold;">Your Clinic Invite Code</p>
                    <p style="margin: 8px 0 0 0; font-family: monospace; font-size: 24px; font-weight: bold; color: #0f172a; letter-spacing: 2px;">{clinic.InviteCode}</p>
                </div>

                <p>Enter this code when signing up in the Triple A mobile app or use the link below to get started:</p>

                <p style="margin: 24px 0;">
                    <a href="{registerLink}" style="background-color: #2563eb; color: #ffffff; padding: 12px 24px; text-decoration: none; border-radius: 6px; font-weight: bold; display: inline-block;">Sign Up & Register</a>
                </p>

                <p style="color: #64748b; font-size: 14px; margin-top: 32px;">Best regards,<br>The Team at {clinic.ClinicName}</p>
            </div>
            """;

        await _emailSender.SendAsync(recipientEmail, subject, body, cancellationToken);

        return new MessageResponseDto($"Invite code successfully sent to {recipientEmail}.");
    }
}
