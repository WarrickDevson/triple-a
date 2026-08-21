namespace KPW.Infrastructure.Services;

public class SendGridOptions
{
    public const string SectionName = "SendGrid";

    /// <summary>
    /// Email provider selection: "SendGrid" to send via SendGrid REST API v3, or "Logging" for dev/logging fallback.
    /// </summary>
    public string Provider { get; set; } = "Logging";

    /// <summary>
    /// Your SendGrid API Key (starts with SG.).
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Verified sender email address configured in your SendGrid account.
    /// </summary>
    public string FromEmail { get; set; } = "noreply@mytriplea.co.za";

    /// <summary>
    /// Verified sender display name.
    /// </summary>
    public string FromName { get; set; } = "Triple A";
}
