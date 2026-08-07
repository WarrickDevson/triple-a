using System.Security.Claims;
using KPW.Application;
using KPW.Application.DTOs.Auth;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using KPW.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace KPW.Api.Controllers;

[ApiController]
[Route("admin")]
[Authorize]
public class AdminController : ControllerBase
{
    private readonly DbContext _dbContext;
    private readonly IEmailSender _emailSender;
    private readonly AppOptions _appOptions;

    public AdminController(
        DbContext dbContext,
        IEmailSender emailSender,
        IOptions<AppOptions> appOptions)
    {
        _dbContext = dbContext;
        _emailSender = emailSender;
        _appOptions = appOptions.Value;
    }

    private bool IsSysAdmin()
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        return role == UserRole.SysAdmin;
    }

    [HttpGet("physios")]
    public async Task<ActionResult<List<PhysioApprovalDto>>> GetPhysios(CancellationToken cancellationToken)
    {
        if (!IsSysAdmin())
        {
            return Forbid();
        }

        var physios = await _dbContext.Set<User>()
            .AsNoTracking()
            .Include(u => u.Clinic)
            .Where(u => u.UserRole == UserRole.Physio)
            .OrderByDescending(u => u.CreatedDate)
            .Select(u => new PhysioApprovalDto(
                u.UserId,
                u.Email,
                u.FirstName,
                u.LastName,
                u.PhoneNumber,
                u.UserRole,
                u.ClinicId,
                u.Clinic != null ? u.Clinic.ClinicName : null,
                u.IsEmailVerified,
                u.IsApproved,
                u.IsActive,
                u.CreatedDate))
            .ToListAsync(cancellationToken);

        return Ok(physios);
    }

    [HttpPost("physios/{userId:int}/approve")]
    public async Task<ActionResult<MessageResponseDto>> ApprovePhysio(int userId, CancellationToken cancellationToken)
    {
        if (!IsSysAdmin())
        {
            return Forbid();
        }

        var physio = await _dbContext.Set<User>()
            .FirstOrDefaultAsync(u => u.UserId == userId && u.UserRole == UserRole.Physio, cancellationToken);

        if (physio is null)
        {
            return NotFound(new { message = "Physio account not found." });
        }

        physio.IsApproved = true;
        physio.IsActive = true;
        await _dbContext.SaveChangesAsync(cancellationToken);

        var loginUrl = $"{_appOptions.PublicPortalUrl.TrimEnd('/')}/login";
        var body = $"""
            <div style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;">
                <h2>Account Approved!</h2>
                <p>Hello {physio.FirstName},</p>
                <p>Great news! Your MoveWell Physiotherapist account has been approved by our administration team.</p>
                <p style="margin: 24px 0;">
                    <a href="{loginUrl}" style="background-color: #10b981; color: #ffffff; padding: 12px 24px; text-decoration: none; border-radius: 6px; font-weight: bold;">Log Into MoveWell Portal</a>
                </p>
            </div>
            """;

        await _emailSender.SendAsync(
            physio.Email,
            "Your MoveWell Physio Account is Approved",
            body,
            cancellationToken);

        return Ok(new MessageResponseDto("Physio account approved successfully."));
    }

    [HttpPost("physios/{userId:int}/reject")]
    public async Task<ActionResult<MessageResponseDto>> RejectPhysio(int userId, CancellationToken cancellationToken)
    {
        if (!IsSysAdmin())
        {
            return Forbid();
        }

        var physio = await _dbContext.Set<User>()
            .FirstOrDefaultAsync(u => u.UserId == userId && u.UserRole == UserRole.Physio, cancellationToken);

        if (physio is null)
        {
            return NotFound(new { message = "Physio account not found." });
        }

        physio.IsApproved = false;
        physio.IsActive = false;
        await _dbContext.SaveChangesAsync(cancellationToken);

        var body = $"""
            <div style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;">
                <h2>MoveWell Registration Update</h2>
                <p>Hello {physio.FirstName},</p>
                <p>Your MoveWell Physiotherapist account registration request was not approved at this time. If you believe this is in error, please contact MoveWell support.</p>
            </div>
            """;

        await _emailSender.SendAsync(
            physio.Email,
            "MoveWell Physio Registration Update",
            body,
            cancellationToken);

        return Ok(new MessageResponseDto("Physio account rejected."));
    }

    [HttpPost("send-physio-invite")]
    public async Task<ActionResult<MessageResponseDto>> SendPhysioInvite([FromBody] SendAdminInviteRequestDto request, CancellationToken cancellationToken)
    {
        if (!IsSysAdmin())
        {
            return Forbid();
        }

        var recipientEmail = request.RecipientEmail.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(recipientEmail))
        {
            return BadRequest(new { message = "Recipient email is required." });
        }

        var clinicName = !string.IsNullOrWhiteSpace(request.ClinicName)
            ? request.ClinicName.Trim()
            : "MoveWell Partner Clinic";

        var inviteCode = "ADMIN-" + Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
        var clinic = new Clinic
        {
            ClinicName = clinicName,
            InviteCode = inviteCode,
            PhysicalAddress = string.Empty,
            ContactNumber = string.Empty
        };

        _dbContext.Set<Clinic>().Add(clinic);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var registerLink = $"{_appOptions.PublicPortalUrl.TrimEnd('/')}/register?inviteCode={Uri.EscapeDataString(inviteCode)}&email={Uri.EscapeDataString(recipientEmail)}";

        var body = $"""
            <div style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;">
                <h2>Invitation to join MoveWell as a Physiotherapist</h2>
                <p>You have been invited by a MoveWell Administrator to join the MoveWell Animal Rehabilitation Platform.</p>
                <p style="margin: 24px 0;">
                    <a href="{registerLink}" style="background-color: #2563eb; color: #ffffff; padding: 12px 24px; text-decoration: none; border-radius: 6px; font-weight: bold;">Accept Invitation & Register</a>
                </p>
                <p>Or copy and paste this URL into your web browser:</p>
                <p><a href="{registerLink}">{registerLink}</a></p>
            </div>
            """;

        await _emailSender.SendAsync(
            recipientEmail,
            "Invitation to join MoveWell Physio Portal",
            body,
            cancellationToken);

        return Ok(new MessageResponseDto("Physio invitation email sent successfully."));
    }
}
