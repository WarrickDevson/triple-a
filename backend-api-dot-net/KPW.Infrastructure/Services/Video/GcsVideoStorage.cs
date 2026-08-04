using Google.Cloud.Storage.V1;
using KPW.Application.Features.Videos.Commands;
using KPW.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KPW.Infrastructure.Services.Video;

public class GcsVideoStorage : IVideoStorage
{
    private readonly StorageClient _storageClient;
    private readonly UrlSigner _urlSigner;
    private readonly VideoOptions _options;
    private readonly ILogger<GcsVideoStorage> _logger;

    public GcsVideoStorage(IOptions<VideoOptions> options, ILogger<GcsVideoStorage> logger)
    {
        _options = options.Value;
        _logger = logger;

        if (string.IsNullOrWhiteSpace(_options.Bucket))
        {
            throw new InvalidOperationException("Video:Bucket is required when Video:Provider is Google.");
        }

        _storageClient = StorageClient.Create();
        _urlSigner = UrlSigner.FromCredential(Google.Apis.Auth.OAuth2.GoogleCredential.GetApplicationDefault());
        _logger.LogInformation("GCS video storage configured for bucket {Bucket}", _options.Bucket);
    }

    public async Task<string> UploadAsync(Stream content, string fileName, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var objectName = $"videos/raw/{Guid.NewGuid():N}{extension}";

        await _storageClient.UploadObjectAsync(
            _options.Bucket,
            objectName,
            GetContentType(extension),
            content,
            cancellationToken: cancellationToken);

        _logger.LogInformation("Uploaded video to gs://{Bucket}/{Object}", _options.Bucket, objectName);
        return objectName;
    }

    public string GetPublicUrl(string storagePath)
    {
        var objectName = NormalizeObjectPath(storagePath);
        var duration = TimeSpan.FromMinutes(Math.Max(1, _options.SignedUrlMinutes));
        return _urlSigner.Sign(_options.Bucket, objectName, duration, HttpMethod.Get);
    }

    public string GetGsUri(string storagePath)
    {
        var objectName = NormalizeObjectPath(storagePath);
        return $"gs://{_options.Bucket}/{objectName}";
    }

    internal static string NormalizeObjectPath(string storagePath)
    {
        if (storagePath.StartsWith("gs://", StringComparison.OrdinalIgnoreCase))
        {
            var withoutScheme = storagePath["gs://".Length..];
            var slashIndex = withoutScheme.IndexOf('/');
            return slashIndex >= 0 ? withoutScheme[(slashIndex + 1)..] : withoutScheme;
        }

        return storagePath.Replace('\\', '/').TrimStart('/');
    }

    private static string GetContentType(string extension) =>
        extension switch
        {
            ".mp4" => "video/mp4",
            ".mov" => "video/quicktime",
            ".hevc" => "video/hevc",
            _ => "application/octet-stream"
        };
}
