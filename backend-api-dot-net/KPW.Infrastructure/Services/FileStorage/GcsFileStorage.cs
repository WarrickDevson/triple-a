using Google.Cloud.Storage.V1;
using KPW.Application.Features.Videos.Commands;
using KPW.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KPW.Infrastructure.Services.FileStorage;

public class GcsFileStorage : IFileStorageService
{
    private readonly StorageClient _storageClient;
    private readonly UrlSigner _urlSigner;
    private readonly VideoOptions _options;
    private readonly ILogger<GcsFileStorage> _logger;

    public GcsFileStorage(IOptions<VideoOptions> options, ILogger<GcsFileStorage> logger)
    {
        _options = options.Value;
        _logger = logger;

        if (string.IsNullOrWhiteSpace(_options.Bucket))
        {
            throw new InvalidOperationException("Video:Bucket is required when Video:Provider is Google.");
        }

        _storageClient = StorageClient.Create();
        _urlSigner = UrlSigner.FromCredential(Google.Apis.Auth.OAuth2.GoogleCredential.GetApplicationDefault());
        _logger.LogInformation("GCS file storage configured for bucket {Bucket}", _options.Bucket);
    }

    public async Task<string> UploadAsync(
        Stream content,
        string fileName,
        string folder = "uploads",
        string? contentType = null,
        CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var cleanFolder = folder.Trim('/', '\\');
        var objectName = $"{cleanFolder}/{Guid.NewGuid():N}{extension}";

        var resolvedContentType = !string.IsNullOrWhiteSpace(contentType)
            ? contentType
            : GetContentType(extension);

        await _storageClient.UploadObjectAsync(
            _options.Bucket,
            objectName,
            resolvedContentType,
            content,
            cancellationToken: cancellationToken);

        _logger.LogInformation("Uploaded file to gs://{Bucket}/{Object} ({ContentType})",
            _options.Bucket, objectName, resolvedContentType);

        return objectName;
    }

    public string GetPublicUrl(string storagePath, TimeSpan? duration = null)
    {
        var objectName = NormalizeObjectPath(storagePath);
        
        // Google Cloud Storage V4 signers allow up to 7 days max
        var maxV4Duration = TimeSpan.FromDays(7);
        var requestedDuration = duration ?? TimeSpan.FromMinutes(Math.Max(60, _options.SignedUrlMinutes));
        if (requestedDuration > maxV4Duration)
        {
            requestedDuration = maxV4Duration;
        }

        return _urlSigner.Sign(_options.Bucket, objectName, requestedDuration, HttpMethod.Get);
    }

    public string GetGsUri(string storagePath)
    {
        var objectName = NormalizeObjectPath(storagePath);
        return $"gs://{_options.Bucket}/{objectName}";
    }

    public string NormalizeObjectPath(string storagePath)
    {
        if (string.IsNullOrWhiteSpace(storagePath)) return string.Empty;

        // If it is a gs:// URI, strip the gs://<bucket>/ prefix
        if (storagePath.StartsWith("gs://", StringComparison.OrdinalIgnoreCase))
        {
            var withoutScheme = storagePath["gs://".Length..];
            var slashIndex = withoutScheme.IndexOf('/');
            return slashIndex >= 0 ? withoutScheme[(slashIndex + 1)..] : withoutScheme;
        }

        // If it is an HTTPS Google Storage URL, extract object path and strip query params
        if (storagePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            storagePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var uri = new Uri(storagePath);
                var path = uri.AbsolutePath.TrimStart('/');
                // Format: /storage/v1/b/{bucket}/o/{object} or /{bucket}/{object}
                if (path.StartsWith(_options.Bucket + "/", StringComparison.OrdinalIgnoreCase))
                {
                    return Uri.UnescapeDataString(path[(_options.Bucket.Length + 1)..]);
                }
                return Uri.UnescapeDataString(path);
            }
            catch
            {
                // Fallback to simple normalization below
            }
        }

        return storagePath.Replace('\\', '/').TrimStart('/');
    }

    private static string GetContentType(string extension) =>
        extension switch
        {
            ".mp4" => "video/mp4",
            ".mov" => "video/quicktime",
            ".hevc" => "video/hevc",
            ".webm" => "video/webm",
            ".pdf" => "application/pdf",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".webp" => "image/webp",
            ".gif" => "image/gif",
            ".wav" => "audio/wav",
            ".mp3" => "audio/mpeg",
            ".m4a" => "audio/mp4",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".txt" => "text/plain",
            _ => "application/octet-stream"
        };
}
