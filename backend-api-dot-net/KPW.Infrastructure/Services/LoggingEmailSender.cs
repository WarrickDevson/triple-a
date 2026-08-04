using KPW.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace KPW.Infrastructure.Services;

public class LoggingEmailSender : IEmailSender
{
    private readonly ILogger<LoggingEmailSender> _logger;

    public LoggingEmailSender(ILogger<LoggingEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "DEV EMAIL — To: {Email} | Subject: {Subject} | Body: {Body}",
            toEmail,
            subject,
            body);
        return Task.CompletedTask;
    }
}
