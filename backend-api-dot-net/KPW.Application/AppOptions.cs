namespace KPW.Application;

public class AppOptions
{
    public const string SectionName = "App";
    public string PublicOwnerAppUrl { get; set; } = "http://localhost:8068";
    public string PublicPortalUrl { get; set; } = "http://localhost:5287/portal";
}
