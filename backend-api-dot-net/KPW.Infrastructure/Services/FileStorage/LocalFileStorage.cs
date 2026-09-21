using KPW.Application.Features.Videos.Commands;
using KPW.Application.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KPW.Infrastructure.Services.FileStorage;

public class LocalFileStorage : IFileStorageService
{
    private readonly string _rootPath;
    private readonly string _publicBasePath;
    private readonly ILogger<LocalFileStorage> _logger;

    public LocalFileStorage(
        IOptions<VideoOptions> options,
        IHostEnvironment environment,
        ILogger<LocalFileStorage> logger)
    {
        _logger = logger;
        var configuredRoot = options.Value.LocalRoot;
        _rootPath = Path.IsPathRooted(configuredRoot)
            ? configuredRoot
            : Path.Combine(environment.ContentRootPath, configuredRoot);

        Directory.CreateDirectory(_rootPath);
        _publicBasePath = "/uploads";
        _logger.LogInformation("Local file storage configured at {RootPath}", _rootPath);
    }

    public async Task<string> UploadAsync(
        Stream content,
        string fileName,
        string folder = "uploads",
        string? contentType = null,
        CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(extension))
        {
            extension = ".bin";
        }

        var cleanFolder = folder.Trim('/', '\\');
        var storedName = $"{Guid.NewGuid():N}{extension}";
        var relativePath = $"{cleanFolder}/{storedName}";
        var fullTargetFolder = Path.Combine(_rootPath, cleanFolder);
        Directory.CreateDirectory(fullTargetFolder);

        var fullPath = Path.Combine(_rootPath, relativePath);

        await using var fileStream = new FileStream(
            fullPath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 81920,
            useAsync: true);

        await content.CopyToAsync(fileStream, cancellationToken);
        _logger.LogInformation("Saved local file to {FullPath}", fullPath);

        return relativePath;
    }

    public string GetPublicUrl(string storagePath, TimeSpan? duration = null)
    {
        if (string.IsNullOrWhiteSpace(storagePath)) return string.Empty;

        if (storagePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            storagePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return storagePath;
        }

        var normalized = storagePath.Replace('\\', '/').TrimStart('/');
        if (normalized.StartsWith("uploads/", StringComparison.OrdinalIgnoreCase))
        {
            return $"/{normalized}";
        }
        return $"{_publicBasePath}/{normalized}";
    }

    public string NormalizeStoragePath(string? storagePath)
    {
        if (string.IsNullOrWhiteSpace(storagePath)) return string.Empty;

        var path = storagePath.Trim();

        // Handle API media endpoint format: /api/media/view?path=...
        if (path.StartsWith("/api/media/view", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("api/media/view", StringComparison.OrdinalIgnoreCase))
        {
            var inner = ExtractQueryParam(path, "path");
            if (!string.IsNullOrWhiteSpace(inner))
            {
                return NormalizeStoragePath(inner);
            }
        }

        if (path.StartsWith("/api/media/", StringComparison.OrdinalIgnoreCase))
        {
            path = path["/api/media/".Length..];
        }
        else if (path.StartsWith("api/media/", StringComparison.OrdinalIgnoreCase))
        {
            path = path["api/media/".Length..];
        }

        if (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var uri = new Uri(path);
                if (uri.AbsolutePath.StartsWith("/api/media/view", StringComparison.OrdinalIgnoreCase))
                {
                    var inner = ExtractQueryParam(uri.Query, "path");
                    if (!string.IsNullOrWhiteSpace(inner))
                    {
                        return NormalizeStoragePath(inner);
                    }
                }
                path = uri.AbsolutePath;
            }
            catch
            {
                // keep path
            }
        }

        var normalized = path.Replace('\\', '/').TrimStart('/');
        if (normalized.StartsWith("uploads/", StringComparison.OrdinalIgnoreCase))
        {
            normalized = normalized["uploads/".Length..];
        }

        return normalized.TrimStart('/');
    }

    public string GetPermanentUrl(string? storagePath)
    {
        if (string.IsNullOrWhiteSpace(storagePath)) return string.Empty;

        if ((storagePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
             storagePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) &&
            !storagePath.Contains("storage.googleapis.com", StringComparison.OrdinalIgnoreCase) &&
            !storagePath.Contains("/api/media", StringComparison.OrdinalIgnoreCase))
        {
            return storagePath;
        }

        var normalized = NormalizeStoragePath(storagePath);
        if (string.IsNullOrWhiteSpace(normalized)) return string.Empty;

        return $"/api/media/view?path={Uri.EscapeDataString(normalized)}";
    }

    public string? GetLocalFilePath(string storagePath)
    {
        var normalized = NormalizeStoragePath(storagePath);
        if (string.IsNullOrWhiteSpace(normalized)) return null;

        return Path.Combine(_rootPath, normalized.Replace('/', Path.DirectorySeparatorChar));
    }

    public string GetFullPath(string storagePath) =>
        Path.Combine(_rootPath, storagePath.Replace('/', Path.DirectorySeparatorChar));

    private static string ExtractQueryParam(string queryOrUrl, string paramName)
    {
        var qIndex = queryOrUrl.IndexOf('?');
        var query = qIndex >= 0 ? queryOrUrl[(qIndex + 1)..] : queryOrUrl;
        foreach (var part in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var eqIndex = part.IndexOf('=');
            var key = eqIndex >= 0 ? part[..eqIndex] : part;
            if (string.Equals(Uri.UnescapeDataString(key), paramName, StringComparison.OrdinalIgnoreCase))
            {
                return eqIndex >= 0 ? Uri.UnescapeDataString(part[(eqIndex + 1)..]) : string.Empty;
            }
        }
        return string.Empty;
    }
}
