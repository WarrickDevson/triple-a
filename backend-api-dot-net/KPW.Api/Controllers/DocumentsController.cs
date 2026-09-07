using KPW.Application.DTOs.SoapNotes;
using KPW.Application.Features.Pets;
using KPW.Application.Features.SoapNotes;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using KPW.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KPW.Api.Controllers;

[ApiController]
[Route("documents")]
[Route("api/documents")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly DbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorageService;

    public DocumentsController(
        DbContext dbContext,
        ICurrentUserService currentUserService,
        IFileStorageService fileStorageService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _fileStorageService = fileStorageService;
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<SharedReportDto>> UploadDocument(
        [FromForm] IFormFile file,
        [FromForm] int petId,
        [FromForm] string title,
        [FromForm] string? category,
        [FromForm] bool shareWithOwner = true,
        CancellationToken cancellationToken = default)
    {
        if (_currentUserService.Role is not (UserRole.Physio or UserRole.SysAdmin))
        {
            return Unauthorized(new { message = "Only physiotherapists and administrators can upload clinical documents." });
        }

        if (file is null || file.Length == 0)
        {
            return BadRequest(new { message = "No file selected." });
        }

        const long maxBytes = 25 * 1024 * 1024; // 25 MB
        if (file.Length > maxBytes)
        {
            return BadRequest(new { message = "File size exceeds 25 MB limit." });
        }

        if (petId <= 0)
        {
            return BadRequest(new { message = "Valid petId is required." });
        }

        await PetAuthorization.EnsureCanAccessPet(_dbContext, _currentUserService, petId, cancellationToken);

        var pet = await _dbContext.Set<Pet>()
            .Include(p => p.Owner)
            .FirstOrDefaultAsync(p => p.PetId == petId, cancellationToken);

        if (pet is null)
        {
            return NotFound(new { message = $"Pet with ID {petId} not found." });
        }

        // Upload to cloud storage in "documents" folder
        await using var stream = file.OpenReadStream();
        var storagePath = await _fileStorageService.UploadAsync(
            stream,
            file.FileName,
            folder: "documents",
            contentType: file.ContentType,
            cancellationToken: cancellationToken);

        var fileUrl = _fileStorageService.GetPublicUrl(storagePath);
        var documentTitle = !string.IsNullOrWhiteSpace(title) ? title.Trim() : Path.GetFileNameWithoutExtension(file.FileName);
        var reportType = !string.IsNullOrWhiteSpace(category) ? category.Trim() : "CLINICAL_DOCUMENT";

        var sharedReport = new SharedReport
        {
            PetId = petId,
            SharedByPhysioId = _currentUserService.UserId!.Value,
            Title = documentTitle,
            ReportType = reportType,
            Summary = $"Uploaded document: {documentTitle} ({reportType})",
            FileUrl = fileUrl,
            IsActive = shareWithOwner,
            SharedAtUtc = DateTime.UtcNow
        };

        _dbContext.Set<SharedReport>().Add(sharedReport);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var created = await _dbContext.Set<SharedReport>()
            .Include(r => r.SharedByPhysio)
            .Include(r => r.Pet)
                .ThenInclude(p => p.Owner)
            .FirstAsync(r => r.SharedReportId == sharedReport.SharedReportId, cancellationToken);

        return Ok(SoapNoteMapper.ToSharedReportDto(created));
    }

    [HttpGet("pet/{petId:int}")]
    public async Task<ActionResult<IReadOnlyList<SharedReportDto>>> GetPetDocuments(
        int petId,
        CancellationToken cancellationToken)
    {
        await PetAuthorization.EnsureCanAccessPet(_dbContext, _currentUserService, petId, cancellationToken);

        var query = _dbContext.Set<SharedReport>()
            .AsNoTracking()
            .Include(r => r.SharedByPhysio)
            .Include(r => r.Pet)
                .ThenInclude(p => p.Owner)
            .Where(r => r.PetId == petId && r.FileUrl != null);

        if (_currentUserService.Role == UserRole.Owner)
        {
            query = query.Where(r => r.IsActive);
        }

        var reports = await query
            .OrderByDescending(r => r.SharedAtUtc)
            .ToListAsync(cancellationToken);

        // Ensure fresh signed URLs for each returned document
        var results = reports.Select(r =>
        {
            var dto = SoapNoteMapper.ToSharedReportDto(r);
            if (!string.IsNullOrWhiteSpace(r.FileUrl))
            {
                var refreshedUrl = _fileStorageService.GetPublicUrl(r.FileUrl);
                return dto with { FileUrl = refreshedUrl };
            }
            return dto;
        }).ToList();

        return Ok(results);
    }

    [HttpGet("{id:int}/download")]
    public async Task<IActionResult> DownloadDocument(int id, CancellationToken cancellationToken)
    {
        var doc = await _dbContext.Set<SharedReport>()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.SharedReportId == id, cancellationToken);

        if (doc is null)
        {
            return NotFound(new { message = $"Document with ID {id} not found." });
        }

        await PetAuthorization.EnsureCanAccessPet(_dbContext, _currentUserService, doc.PetId, cancellationToken);

        if (string.IsNullOrWhiteSpace(doc.FileUrl))
        {
            return BadRequest(new { message = "This report does not contain an external file attachment." });
        }

        var freshUrl = _fileStorageService.GetPublicUrl(doc.FileUrl);
        return Redirect(freshUrl);
    }
}
