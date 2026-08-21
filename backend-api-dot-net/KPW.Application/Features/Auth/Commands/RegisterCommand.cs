using KPW.Application.DTOs.Auth;
using KPW.Application.Features.Auth;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using KPW.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KPW.Application.Features.Auth.Commands;

public record RegisterCommand(RegisterRequestDto Request) : IRequest<AuthResponseDto>;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponseDto>
{
    private readonly DbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IEmailSender _emailSender;
    private readonly AppOptions _appOptions;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        DbContext dbContext,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IEmailSender emailSender,
        IOptions<AppOptions> appOptions,
        ILogger<RegisterCommandHandler> logger)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _emailSender = emailSender;
        _appOptions = appOptions.Value;
        _logger = logger;
    }

    public async Task<AuthResponseDto> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var requestedRole = request.Role?.Trim();
        var userRole = string.Equals(requestedRole, UserRole.Owner, StringComparison.OrdinalIgnoreCase)
            || (!string.IsNullOrWhiteSpace(request.InviteCode) && string.IsNullOrWhiteSpace(request.ClinicName))
            ? UserRole.Owner
            : UserRole.Physio;

        var rawInviteCode = request.InviteCode?.Trim().ToUpperInvariant();
        Clinic? clinic = null;
        bool isApproved = true;

        _logger.LogInformation(
            "Processing registration for {Email} with role {Role}. InviteCode: {InviteCode}, ClinicName: {ClinicName}",
            request.Email,
            userRole,
            string.IsNullOrWhiteSpace(rawInviteCode) ? "(none - optional)" : rawInviteCode,
            string.IsNullOrWhiteSpace(request.ClinicName) ? "(auto-generated)" : request.ClinicName);

        if (!string.IsNullOrWhiteSpace(rawInviteCode))
        {
            clinic = await _dbContext.Set<Clinic>()
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.InviteCode == rawInviteCode, cancellationToken);

            if (clinic is null)
            {
                _logger.LogWarning("Registration failed for {Email}: Invalid clinic invite code '{InviteCode}'", request.Email, rawInviteCode);
                throw new InvalidOperationException("Invalid clinic invite code.");
            }

            _logger.LogInformation("Matched existing clinic '{ClinicName}' (ID: {ClinicId}) for {Email}", clinic.ClinicName, clinic.ClinicId, request.Email);
        }
        else
        {
            if (userRole == UserRole.Owner)
            {
                _logger.LogWarning("Owner registration rejected for {Email}: missing required clinic invite code", request.Email);
                throw new InvalidOperationException("Clinic invite code is required for pet owner registration.");
            }

            // Physio self-registering a new clinic -> create Clinic entry, require Admin approval
            var clinicName = !string.IsNullOrWhiteSpace(request.ClinicName)
                ? request.ClinicName.Trim()
                : $"{request.FirstName.Trim()} {request.LastName.Trim()}'s Clinic";

            var generatedInviteCode = "TA-" + Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();

            clinic = new Clinic
            {
                ClinicName = clinicName,
                InviteCode = generatedInviteCode,
                PhysicalAddress = string.Empty,
                ContactNumber = request.PhoneNumber?.Trim() ?? string.Empty
            };

            _dbContext.Set<Clinic>().Add(clinic);
            await _dbContext.SaveChangesAsync(cancellationToken);

            isApproved = false; // Requires SysAdmin approval
            _logger.LogInformation("Created new clinic '{ClinicName}' (ID: {ClinicId}, InviteCode: {InviteCode}) for self-registered physio {Email}", clinicName, clinic.ClinicId, generatedInviteCode, request.Email);
        }

        var users = _dbContext.Set<User>();
        var email = request.Email.Trim().ToLowerInvariant();
        var emailExists = await users.AnyAsync(u => u.Email == email, cancellationToken);
        if (emailExists)
        {
            _logger.LogWarning("Registration failed for {Email}: email already exists in database", email);
            throw new InvalidOperationException("Email is already registered.");
        }

        var rawVerificationToken = _jwtTokenService.GenerateRefreshToken();
        var verificationTokenHash = _jwtTokenService.HashRefreshToken(rawVerificationToken);

        var user = new User
        {
            Email = email,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            PhoneNumber = request.PhoneNumber?.Trim(),
            UserRole = userRole,
            SubscriptionTier = SubscriptionTier.Free,
            ClinicId = clinic.ClinicId,
            IsEmailVerified = false,
            EmailVerificationTokenHash = verificationTokenHash,
            EmailVerificationTokenExpiresAt = DateTime.UtcNow.AddHours(24),
            IsApproved = isApproved
        };

        users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User created successfully (ID: {UserId}, Email: {Email}, Role: {Role}, IsApproved: {IsApproved})", user.UserId, user.Email, user.UserRole, user.IsApproved);

        try
        {
            await SendVerificationEmailAsync(user, rawVerificationToken, cancellationToken);
            _logger.LogInformation("Verification email successfully dispatched for {Email}", user.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send initial verification email to {Email}", user.Email);
        }

        return await BuildAuthResponse(user, clinic, cancellationToken);
    }

    private async Task SendVerificationEmailAsync(User user, string rawToken, CancellationToken cancellationToken)
    {
        var baseUrl = user.UserRole == UserRole.Owner
            ? _appOptions.PublicOwnerAppUrl.TrimEnd('/')
            : _appOptions.PublicPortalUrl.TrimEnd('/');
        var verifyLink = $"{baseUrl}/verify-email?email={Uri.EscapeDataString(user.Email)}&token={Uri.EscapeDataString(rawToken)}";

        _logger.LogInformation("Generated verification link for {Email}: {VerifyLink}", user.Email, verifyLink);

        var body = $"""
            <div style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;">
                <h2>Welcome to Triple A, {user.FirstName}!</h2>
                <p>Thank you for signing up. Please verify your email address to complete your account setup.</p>
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
            "Verify your Triple A account",
            body,
            cancellationToken);
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
