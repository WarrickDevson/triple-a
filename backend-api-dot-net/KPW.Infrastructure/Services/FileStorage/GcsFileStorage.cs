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

        _ = Task.Run(async () =>
        {
            try
            {
                var bucket = await _storageClient.GetBucketAsync(_options.Bucket);
                var hasCors = bucket.Cors != null && bucket.Cors.Any(c => c.Origin != null && c.Origin.Contains("*"));
                if (!hasCors)
                {
                    bucket.Cors ??= new List<Google.Apis.Storage.v1.Data.Bucket.CorsData>();
                    bucket.Cors.Add(new Google.Apis.Storage.v1.Data.Bucket.CorsData
                    {
                        Origin = new List<string> { "*" },
                        Method = new List<string> { "GET", "HEAD", "PUT", "POST", "DELETE", "OPTIONS" },
                        ResponseHeader = new List<string> { "*" },
                        MaxAgeSeconds = 3600
                    });
                    await _storageClient.UpdateBucketAsync(bucket);
                    _logger.LogInformation("Successfully configured CORS on GCS bucket {Bucket}", _options.Bucket);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not automatically configure CORS on GCS bucket {Bucket}: {Message}", _options.Bucket, ex.Message);
            }
        });
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
        if (string.IsNullOrWhiteSpace(storagePath)) return string.Empty;

        // If it is an external URL not from our bucket, return directly
        if ((storagePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
             storagePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) &&
            !storagePath.Contains(_options.Bucket, StringComparison.OrdinalIgnoreCase))
        {
            return storagePath;
        }

        var objectName = NormalizeObjectPath(storagePath);
        if (string.IsNullOrWhiteSpace(objectName)) return storagePath;
        
        // Google Cloud Storage V4 signers allow up to 7 days max
        var maxV4Duration = TimeSpan.FromDays(7);
        var requestedDuration = duration ?? TimeSpan.FromMinutes(Math.Max(60, _options.SignedUrlMinutes));
        if (requestedDuration > maxV4Duration)
        {
            requestedDuration = maxV4Duration;
        }

        try
        {
            return _urlSigner.Sign(_options.Bucket, objectName, requestedDuration, HttpMethod.Get);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to sign URL for GCS object {Object}, returning original path", objectName);
            return storagePath;
        }
    }

    public string GetGsUri(string storagePath)
    {
        var objectName = NormalizeObjectPath(storagePath);
        return $"gs://{_options.Bucket}/{objectName}";
    }

    public string NormalizeObjectPath(string storagePath)
    {
        if (string.IsNullOrWhiteSpace(storagePath)) return string.Empty;

        var trimmed = storagePath.Trim();

        // Handle API media endpoint format: /api/media/view?path=...
        if (trimmed.StartsWith("/api/media/view", StringComparison.OrdinalIgnoreCase) ||
            trimmed.StartsWith("api/media/view", StringComparison.OrdinalIgnoreCase))
        {
            var inner = ExtractQueryParam(trimmed, "path");
            if (!string.IsNullOrWhiteSpace(inner))
            {
                return NormalizeObjectPath(inner);
            }
        }

        if (trimmed.StartsWith("/api/media/", StringComparison.OrdinalIgnoreCase))
        {
            trimmed = trimmed["/api/media/".Length..];
        }
        else if (trimmed.StartsWith("api/media/", StringComparison.OrdinalIgnoreCase))
        {
            trimmed = trimmed["api/media/".Length..];
        }

        // If it is a gs:// URI, strip the gs://<bucket>/ prefix
        if (trimmed.StartsWith("gs://", StringComparison.OrdinalIgnoreCase))
        {
            var withoutScheme = trimmed["gs://".Length..];
            var slashIndex = withoutScheme.IndexOf('/');
            return slashIndex >= 0 ? withoutScheme[(slashIndex + 1)..] : withoutScheme;
        }

        // If it is an HTTPS Google Storage URL, extract object path and strip query params
        if (trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var uri = new Uri(trimmed);
                if (uri.AbsolutePath.StartsWith("/api/media/view", StringComparison.OrdinalIgnoreCase))
                {
                    var inner = ExtractQueryParam(uri.Query, "path");
                    if (!string.IsNullOrWhiteSpace(inner))
                    {
                        return NormalizeObjectPath(inner);
                    }
                }

                var path = uri.AbsolutePath.TrimStart('/');
                // Format: /storage/v1/b/{bucket}/o/{object} or /{bucket}/{object}
                if (path.StartsWith(_options.Bucket + "/", StringComparison.OrdinalIgnoreCase))
                {
                    return Uri.UnescapeDataString(path[(_options.Bucket.Length + 1)..]);
                }
                if (path.Contains("/o/", StringComparison.OrdinalIgnoreCase))
                {
                    var oIndex = path.IndexOf("/o/", StringComparison.OrdinalIgnoreCase);
                    return Uri.UnescapeDataString(path[(oIndex + 3)..]);
                }
                return Uri.UnescapeDataString(path);
            }
            catch
            {
                // Fallback to simple normalization below
            }
        }

        var normalized = trimmed.Replace('\\', '/').TrimStart('/');
        if (normalized.StartsWith("uploads/", StringComparison.OrdinalIgnoreCase))
        {
            normalized = normalized["uploads/".Length..];
        }
        return normalized.TrimStart('/');
    }

    public string NormalizeStoragePath(string? storagePath) =>
        string.IsNullOrWhiteSpace(storagePath) ? string.Empty : NormalizeObjectPath(storagePath);

    public string GetPermanentUrl(string? storagePath)
    {
        if (string.IsNullOrWhiteSpace(storagePath)) return string.Empty;

        // If it is an external URL not from our bucket or domain, leave it untouched
        if ((storagePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
             storagePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) &&
            !storagePath.Contains("storage.googleapis.com", StringComparison.OrdinalIgnoreCase) &&
            !storagePath.Contains(_options.Bucket, StringComparison.OrdinalIgnoreCase) &&
            !storagePath.Contains("/api/media", StringComparison.OrdinalIgnoreCase))
        {
            return storagePath;
        }

        var normalized = NormalizeStoragePath(storagePath);
        if (string.IsNullOrWhiteSpace(normalized)) return string.Empty;

        return $"/api/media/view?path={Uri.EscapeDataString(normalized)}";
    }

    public string? GetLocalFilePath(string storagePath) => null;

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
