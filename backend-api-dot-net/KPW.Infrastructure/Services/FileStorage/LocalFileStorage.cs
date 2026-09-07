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

    public string GetFullPath(string storagePath) =>
        Path.Combine(_rootPath, storagePath.Replace('/', Path.DirectorySeparatorChar));
}
