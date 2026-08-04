using KPW.Application.Features.Videos.Commands;
using KPW.Application.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KPW.Infrastructure.Services.Video;

public class LocalVideoStorage : IVideoStorage
{
    private readonly string _rootPath;
    private readonly string _publicBasePath;

    public LocalVideoStorage(IOptions<VideoOptions> options, IHostEnvironment environment)
    {
        var configuredRoot = options.Value.LocalRoot;
        _rootPath = Path.IsPathRooted(configuredRoot)
            ? configuredRoot
            : Path.Combine(environment.ContentRootPath, configuredRoot);

        Directory.CreateDirectory(Path.Combine(_rootPath, "videos"));
        _publicBasePath = "/uploads";
    }

    public async Task<string> UploadAsync(Stream content, string fileName, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var storedName = $"{Guid.NewGuid():N}{extension}";
        var relativePath = $"videos/{storedName}";
        var fullPath = Path.Combine(_rootPath, relativePath);

        await using var fileStream = new FileStream(
            fullPath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 81920,
            useAsync: true);

        await content.CopyToAsync(fileStream, cancellationToken);
        return relativePath;
    }

    public string GetPublicUrl(string storagePath)
    {
        var normalized = storagePath.Replace('\\', '/').TrimStart('/');
        return $"{_publicBasePath}/{normalized}";
    }

    public string GetFullPath(string storagePath) =>
        Path.Combine(_rootPath, storagePath.Replace('/', Path.DirectorySeparatorChar));
}
