using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KPW.Api.Controllers;

[ApiController]
[Route("api/media")]
[AllowAnonymous]
public class MediaController : ControllerBase
{
    private readonly IFileStorageService _fileStorageService;
    private readonly DbContext _dbContext;
    private readonly ILogger<MediaController> _logger;

    private static readonly HashSet<string> AllowedFolders = new(StringComparer.OrdinalIgnoreCase)
    {
        "avatars",
        "pets",
        "exercise-images",
        "exercise-videos",
        "videos",
        "documents",
        "attachments",
        "uploads"
    };

    public MediaController(
        IFileStorageService fileStorageService,
        DbContext dbContext,
        ILogger<MediaController> logger)
    {
        _fileStorageService = fileStorageService;
        _dbContext = dbContext;
        _logger = logger;
    }

    [HttpGet("view")]
    public Task<IActionResult> ViewMedia(
        [FromQuery] string? path,
        [FromQuery] bool download = false,
        [FromQuery] string? fileName = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return Task.FromResult<IActionResult>(BadRequest(new { message = "Media path is required." }));
        }

        return ServeStoragePathAsync(path, download, fileName, cancellationToken);
    }

    [HttpGet("avatar/{userId:int}")]
    public async Task<IActionResult> GetUserAvatar(
        int userId,
        [FromQuery] bool download = false,
        CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Set<User>()
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);

        if (user == null || string.IsNullOrWhiteSpace(user.ProfilePictureUrl))
        {
            return NotFound(new { message = "User avatar not found." });
        }

        return await ServeStoragePathAsync(user.ProfilePictureUrl, download, $"{user.FirstName}_{user.LastName}_avatar", cancellationToken);
    }

    [HttpGet("pet/{petId:int}")]
    public async Task<IActionResult> GetPetPhoto(
        int petId,
        [FromQuery] bool download = false,
        CancellationToken cancellationToken = default)
    {
        var pet = await _dbContext.Set<Pet>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PetId == petId, cancellationToken);

        if (pet == null || string.IsNullOrWhiteSpace(pet.ProfilePictureUrl))
        {
            return NotFound(new { message = "Pet photo not found." });
        }

        return await ServeStoragePathAsync(pet.ProfilePictureUrl, download, $"{pet.PetName}_photo", cancellationToken);
    }

    [HttpGet("exercise/{exerciseId:int}/cover")]
    public async Task<IActionResult> GetExerciseCover(
        int exerciseId,
        [FromQuery] bool download = false,
        CancellationToken cancellationToken = default)
    {
        var exercise = await _dbContext.Set<Exercise>()
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.ExerciseId == exerciseId, cancellationToken);

        if (exercise == null || string.IsNullOrWhiteSpace(exercise.CoverImageUrl))
        {
            return NotFound(new { message = "Exercise cover image not found." });
        }

        return await ServeStoragePathAsync(exercise.CoverImageUrl, download, $"{exercise.Title}_cover", cancellationToken);
    }

    [HttpGet("exercise/{exerciseId:int}/video")]
    public async Task<IActionResult> GetExerciseVideo(
        int exerciseId,
        [FromQuery] bool download = false,
        CancellationToken cancellationToken = default)
    {
        var exercise = await _dbContext.Set<Exercise>()
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.ExerciseId == exerciseId, cancellationToken);

        if (exercise == null || string.IsNullOrWhiteSpace(exercise.VideoUrl))
        {
            return NotFound(new { message = "Exercise video not found." });
        }

        return await ServeStoragePathAsync(exercise.VideoUrl, download, $"{exercise.Title}_video", cancellationToken);
    }

    [HttpGet("document/{sharedReportId:int}")]
    public async Task<IActionResult> GetDocument(
        int sharedReportId,
        [FromQuery] bool download = false,
        CancellationToken cancellationToken = default)
    {
        var report = await _dbContext.Set<SharedReport>()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.SharedReportId == sharedReportId, cancellationToken);

        if (report == null || string.IsNullOrWhiteSpace(report.FileUrl))
        {
            return NotFound(new { message = "Report document file not found." });
        }

        var fileName = !string.IsNullOrWhiteSpace(report.Title)
            ? $"{report.Title.Trim().Replace(' ', '_')}.pdf"
            : $"report_{sharedReportId}.pdf";

        return await ServeStoragePathAsync(report.FileUrl, download, fileName, cancellationToken);
    }

    [HttpGet("{*storagePath}")]
    public Task<IActionResult> GetCatchAll(
        string storagePath,
        [FromQuery] bool download = false,
        [FromQuery] string? fileName = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(storagePath))
        {
            return Task.FromResult<IActionResult>(BadRequest(new { message = "Media path is required." }));
        }

        return ServeStoragePathAsync(storagePath, download, fileName, cancellationToken);
    }

    private Task<IActionResult> ServeStoragePathAsync(
        string rawPath,
        bool download,
        string? customFileName,
        CancellationToken cancellationToken)
    {
        var normalized = _fileStorageService.NormalizeStoragePath(rawPath);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return Task.FromResult<IActionResult>(BadRequest(new { message = "Invalid storage path." }));
        }

        // Security check: Prevent directory traversal
        if (normalized.Contains("..") || normalized.Contains('\\') || normalized.Contains(':'))
        {
            _logger.LogWarning("Potential path traversal blocked: {RawPath}", rawPath);
            return Task.FromResult<IActionResult>(BadRequest(new { message = "Invalid path syntax." }));
        }

        // Security check: Restrict to allowed root directories
        var firstSlashIndex = normalized.IndexOf('/');
        var rootFolder = firstSlashIndex > 0 ? normalized[..firstSlashIndex] : normalized;
        if (!AllowedFolders.Contains(rootFolder))
        {
            _logger.LogWarning("Access to folder '{Folder}' denied for path: {Normalized}", rootFolder, normalized);
            return Task.FromResult<IActionResult>(BadRequest(new { message = "Access to requested media folder is not permitted." }));
        }

        // Local Storage handling
        var localFilePath = _fileStorageService.GetLocalFilePath(normalized);
        if (localFilePath != null)
        {
            if (!System.IO.File.Exists(localFilePath))
            {
                return Task.FromResult<IActionResult>(NotFound(new { message = "Media file not found on server." }));
            }

            var extension = Path.GetExtension(localFilePath).ToLowerInvariant();
            var contentType = GetContentType(extension);

            Response.Headers.CacheControl = "public, max-age=86400"; // 24 hours client cache

            if (download)
            {
                var downloadName = !string.IsNullOrWhiteSpace(customFileName)
                    ? (customFileName.EndsWith(extension, StringComparison.OrdinalIgnoreCase) ? customFileName : $"{customFileName}{extension}")
                    : Path.GetFileName(localFilePath);

                return Task.FromResult<IActionResult>(PhysicalFile(localFilePath, contentType, downloadName));
            }

            return Task.FromResult<IActionResult>(PhysicalFile(localFilePath, contentType, enableRangeProcessing: true));
        }

        // Cloud Storage (GCS) handling: 302 redirect with fresh signed URL
        var signedUrl = _fileStorageService.GetPublicUrl(normalized, TimeSpan.FromMinutes(60));
        if (string.IsNullOrWhiteSpace(signedUrl) || signedUrl.Equals(normalized, StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<IActionResult>(NotFound(new { message = "Media file not available." }));
        }

        Response.Headers.CacheControl = "public, max-age=900"; // Cache the redirect for 15 minutes
        return Task.FromResult<IActionResult>(Redirect(signedUrl));
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
