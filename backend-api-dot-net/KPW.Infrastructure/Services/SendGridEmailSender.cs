using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using KPW.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KPW.Infrastructure.Services;

public class SendGridEmailSender : IEmailSender
{
    private const string SendGridApiUrl = "https://api.sendgrid.com/v3/mail/send";
    private readonly HttpClient _httpClient;
    private readonly SendGridOptions _options;
    private readonly LoggingEmailSender _fallbackSender;
    private readonly ILogger<SendGridEmailSender> _logger;

    public SendGridEmailSender(
        HttpClient httpClient,
        IOptions<SendGridOptions> options,
        LoggingEmailSender fallbackSender,
        ILogger<SendGridEmailSender> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _fallbackSender = fallbackSender;
        _logger = logger;
    }

    public async Task SendAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default)
    {
        var rawKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY") 
            ?? Environment.GetEnvironmentVariable("SendGrid__ApiKey") 
            ?? Environment.GetEnvironmentVariable("DEPLOY_SENDGRID_API_KEY")
            ?? _options.ApiKey;

        var apiKey = rawKey?.Trim().Trim('"', '\'', ' ', '\r', '\n');

        var rawProvider = Environment.GetEnvironmentVariable("SENDGRID_PROVIDER") 
            ?? Environment.GetEnvironmentVariable("SendGrid__Provider") 
            ?? _options.Provider;
        var provider = !string.IsNullOrWhiteSpace(rawProvider) ? rawProvider.Trim() : "Logging";

        var rawFromEmail = Environment.GetEnvironmentVariable("SENDGRID_FROM_EMAIL") 
            ?? Environment.GetEnvironmentVariable("SendGrid__FromEmail") 
            ?? Environment.GetEnvironmentVariable("DEPLOY_SENDGRID_FROM_EMAIL")
            ?? _options.FromEmail;
        var fromEmail = !string.IsNullOrWhiteSpace(rawFromEmail) ? rawFromEmail.Trim().Trim('"', '\'', ' ', '\r', '\n') : "noreply@mytriplea.co.za";

        var rawFromName = Environment.GetEnvironmentVariable("SENDGRID_FROM_NAME") 
            ?? Environment.GetEnvironmentVariable("SendGrid__FromName") 
            ?? Environment.GetEnvironmentVariable("DEPLOY_SENDGRID_FROM_NAME")
            ?? _options.FromName;
        var fromName = !string.IsNullOrWhiteSpace(rawFromName) ? rawFromName.Trim().Trim('"', '\'', ' ', '\r', '\n') : "Triple A";

        var isSendGridMode = provider.Equals("SendGrid", StringComparison.OrdinalIgnoreCase) || !string.IsNullOrWhiteSpace(apiKey);
        var hasApiKey = !string.IsNullOrWhiteSpace(apiKey);

        if (!isSendGridMode || !hasApiKey)
        {
            _logger.LogInformation(
                "SendGrid API Key is not configured or provider is set to '{Provider}'. Falling back to LoggingEmailSender.",
                provider);

            await _fallbackSender.SendAsync(toEmail, subject, body, cancellationToken);
            return;
        }

        try
        {
            var payload = new
            {
                personalizations = new[]
                {
                    new
                    {
                        to = new[]
                        {
                            new { email = toEmail }
                        }
                    }
                },
                from = new
                {
                    email = fromEmail,
                    name = fromName
                },
                subject = subject,
                content = new[]
                {
                    new
                    {
                        type = "text/html",
                        value = body
                    }
                }
            };

            var jsonContent = JsonSerializer.Serialize(payload);
            using var request = new HttpRequestMessage(HttpMethod.Post, SendGridApiUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            _logger.LogInformation(
                "Dispatching email via SendGrid: from '{FromEmail}' ({FromName}) to '{ToEmail}' with subject '{Subject}'",
                fromEmail,
                fromName,
                toEmail,
                subject);

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Email successfully sent via SendGrid to {Email}", toEmail);
            }
            else
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError(
                    "Failed to send email via SendGrid. StatusCode: {StatusCode}, Error: {ErrorBody}. Logging message body to fallback logger.",
                    response.StatusCode,
                    errorBody);

                await _fallbackSender.SendAsync(toEmail, subject, body, cancellationToken);

                throw new InvalidOperationException($"Failed to send email via SendGrid (Status {response.StatusCode}): {errorBody}");
            }
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            _logger.LogError(ex, "Exception encountered while sending email via SendGrid to {Email}", toEmail);
            throw;
        }
    }
}
