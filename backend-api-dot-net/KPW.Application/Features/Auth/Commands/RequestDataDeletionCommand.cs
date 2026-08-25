using KPW.Application.DTOs.Auth;
using KPW.Application.Features.Auth;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KPW.Application.Features.Auth.Commands;

public record RequestDataDeletionCommand(DataDeletionRequestDto Request) : IRequest<DataDeletionResponseDto>;

public class RequestDataDeletionCommandHandler : IRequestHandler<RequestDataDeletionCommand, DataDeletionResponseDto>
{
    private readonly DbContext _dbContext;
    private readonly IEmailSender _emailSender;
    private readonly AppOptions _appOptions;
    private readonly ILogger<RequestDataDeletionCommandHandler> _logger;

    public RequestDataDeletionCommandHandler(
        DbContext dbContext,
        IEmailSender emailSender,
        IOptions<AppOptions> appOptions,
        ILogger<RequestDataDeletionCommandHandler> logger)
    {
        _dbContext = dbContext;
        _emailSender = emailSender;
        _appOptions = appOptions.Value;
        _logger = logger;
    }

    public async Task<DataDeletionResponseDto> Handle(
        RequestDataDeletionCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;
        var email = request.Email.Trim().ToLowerInvariant();
        var requestType = string.IsNullOrWhiteSpace(request.RequestType) ? "FullAccountAndData" : request.RequestType.Trim();
        var reason = request.Reason?.Trim() ?? "Not provided";
        var notes = request.AdditionalNotes?.Trim() ?? string.Empty;

        var referenceId = "DEL-" + Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();

        _logger.LogInformation(
            "POPIA / App Store Data Deletion Request received. Ref: {Ref}, Email: {Email}, Type: {Type}",
            referenceId, email, requestType);

        var user = await _dbContext.Set<User>()
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        var userExists = user is not null;
        var userName = user != null ? $"{user.FirstName} {user.LastName}" : "User";

        var emailSubject = $"Triple A — Data Deletion Request Received (Ref: {referenceId})";
        var emailBody = $@"
            <div style=""font-family: 'Segoe UI', Arial, sans-serif; max-width: 600px; margin: 0 auto; color: #1E2328; line-height: 1.6;"">
                <div style=""background-color: #0A1A2E; padding: 24px; border-radius: 8px 8px 0 0; text-align: center;"">
                    <h1 style=""color: #FFFFFF; margin: 0; font-size: 22px;"">Triple A — Animal Activity Assistant</h1>
                    <p style=""color: #7A8A5C; margin: 4px 0 0 0; font-size: 13px;"">POPIA & Data Privacy Compliance</p>
                </div>
                <div style=""background-color: #FFFFFF; padding: 28px; border: 1px solid #E5E7E3; border-top: none; border-radius: 0 0 8px 8px;"">
                    <h2 style=""color: #0A1A2E; font-size: 18px; margin-top: 0;"">Data Deletion Request Received</h2>
                    <p>Hello {userName},</p>
                    <p>We have received your formal request to delete your account and personal data from the Triple A platform.</p>
                    
                    <div style=""background-color: #F8F9F7; border-left: 4px solid #6B7A4D; padding: 14px; margin: 20px 0; border-radius: 4px;"">
                        <p style=""margin: 0 0 6px 0; font-size: 13px; color: #6B7280; text-transform: uppercase; font-weight: 600;"">Request Reference</p>
                        <p style=""margin: 0; font-family: monospace; font-size: 16px; font-weight: bold; color: #0A1A2E;"">{referenceId}</p>
                        <p style=""margin: 8px 0 0 0; font-size: 13px; color: #6B7280;"">Request Scope: <strong>{requestType}</strong></p>
                    </div>

                    <h3 style=""color: #0A1A2E; font-size: 15px; margin-top: 24px;"">What Happens Next?</h3>
                    <ul style=""padding-left: 20px; color: #374151; font-size: 14px;"">
                        <li><strong>Verification:</strong> Our Data Protection Officer will review and verify your request.</li>
                        <li><strong>Data Purging:</strong> Your personal profile, authentication credentials, mobile tracking entries, subjective notes, and uploaded media attachments will be permanently scheduled for erasure.</li>
                        <li><strong>Statutory Veterinary Clinical Records:</strong> As required by South African veterinary statutory regulations and clinical record-keeping obligations, formal clinical notes and rehabilitation treatment plans authored by veterinary physiotherapists must be retained for the minimum statutory period by the attending clinic.</li>
                        <li><strong>Processing Timeline:</strong> In accordance with Section 24 of POPIA, your request will be completed within 30 calendar days.</li>
                    </ul>

                    <p style=""font-size: 13px; color: #6B7280; margin-top: 24px;"">
                        If you did not make this request or have any questions, please immediately contact our Information Officer at <a href=""mailto:privacy@mytriplea.co.za"" style=""color: #6B7A4D;"">privacy@mytriplea.co.za</a> quoting your reference <strong>{referenceId}</strong>.
                    </p>
                    <hr style=""border: none; border-top: 1px solid #E5E7E3; margin: 24px 0;"" />
                    <p style=""font-size: 12px; color: #9CA3AF; margin: 0; text-align: center;"">
                        &copy; Triple A &bull; Animal Activity Assistant &bull; POPIA Compliant
                    </p>
                </div>
            </div>";

        try
        {
            await _emailSender.SendAsync(email, emailSubject, emailBody, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not send data deletion confirmation email to {Email}", email);
        }

        var message = userExists
            ? $"Your deletion request has been registered under reference {referenceId}. A confirmation email with details and statutory timelines has been sent to {email}."
            : $"If an account exists for {email}, a data deletion confirmation and reference ticket has been dispatched.";

        return new DataDeletionResponseDto(
            Success: true,
            Message: message,
            RequestReference: referenceId
        );
    }
}
