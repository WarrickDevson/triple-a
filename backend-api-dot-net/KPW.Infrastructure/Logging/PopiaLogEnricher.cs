using System.Text.RegularExpressions;

namespace KPW.Infrastructure.Logging;

public static class PopiaLogEnricher
{
    private static readonly Regex Ipv4Regex = new(
        @"\b(?:(?:25[0-5]|2[0-4]\d|[01]?\d\d?)\.){3}(?:25[0-5]|2[0-4]\d|[01]?\d\d?)\b",
        RegexOptions.Compiled);

    public static string MaskIpAddresses(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return Ipv4Regex.Replace(value, "xxx.xxx.xxx.xxx");
    }
}
