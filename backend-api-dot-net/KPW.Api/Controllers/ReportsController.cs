using KPW.Application.DTOs.Reports;
using KPW.Application.DTOs.SoapNotes;
using KPW.Application.Features.Reports.Commands;
using KPW.Application.Features.Reports.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KPW.Api.Controllers;

[ApiController]
[Route("reports")]
[Route("api/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReportsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [HttpGet("recent")]
    public async Task<ActionResult<IReadOnlyList<SharedReportDto>>> GetRecentReports(
        [FromQuery] int? petId,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new GetRecentClinicReportsQuery(petId), cancellationToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpGet("pet/{petId:int}/download")]
    public async Task<IActionResult> DownloadPetReport(
        int petId,
        [FromQuery] string? type,
        [FromQuery] string? customTitle,
        [FromQuery] string? summary,
        [FromQuery] string? dischargeStatus,
        [FromQuery] string? maintenancePlan,
        [FromQuery] string? veterinarianNotes,
        [FromQuery] string? ownerInstructions,
        [FromQuery] int? soapNoteId,
        [FromQuery] DateOnly? periodFrom,
        [FromQuery] DateOnly? periodTo,
        [FromQuery] string? sessionsJson,
        CancellationToken cancellationToken)
    {
        try
        {
            IReadOnlyList<ReferencedReportSessionDto>? referencedSessions = null;
            if (!string.IsNullOrWhiteSpace(sessionsJson))
            {
                try
                {
                    referencedSessions = System.Text.Json.JsonSerializer.Deserialize<List<ReferencedReportSessionDto>>(
                        sessionsJson,
                        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                catch
                {
                    // Ignore malformed JSON and allow query fallback
                }
            }

            var query = new GeneratePetReportQuery(
                petId,
                type,
                customTitle,
                summary,
                dischargeStatus,
                maintenancePlan,
                veterinarianNotes,
                ownerInstructions,
                soapNoteId,
                periodFrom,
                periodTo,
                referencedSessions);

            var result = await _mediator.Send(query, cancellationToken);
            return File(result.Content, "application/pdf", result.FileName);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("shared/{sharedReportId:int}/download")]
    public async Task<IActionResult> DownloadSharedReport(
        int sharedReportId,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new GenerateSharedReportPdfQuery(sharedReportId), cancellationToken);
            return File(result.Content, "application/pdf", result.FileName);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("create")]
    [HttpPost("generate")]
    public async Task<ActionResult<SharedReportDto>> CreateReport(
        [FromBody] CreateReportRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new CreateReportCommand(request), cancellationToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("pet/{petId:int}/shared")]
    public async Task<ActionResult<IReadOnlyList<SharedReportDto>>> GetSharedReports(
        int petId,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new GetSharedReportsByPetQuery(petId), cancellationToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost("pet/{petId:int}/share-document")]
    public async Task<ActionResult<SharedReportDto>> ShareDocument(
        int petId,
        [FromBody] ShareDocumentRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new ShareDocumentCommand(petId, request), cancellationToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("pet/{petId:int}/publish-progress-report")]
    public async Task<ActionResult<SharedReportDto>> PublishProgressReport(
        int petId,
        [FromQuery] string? title,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new PublishProgressReportCommand(petId, title), cancellationToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpDelete("shared/{sharedReportId:int}")]
    public async Task<IActionResult> DeleteSharedReport(
        int sharedReportId,
        CancellationToken cancellationToken)
    {
        var success = await _mediator.Send(new DeleteSharedReportCommand(sharedReportId), cancellationToken);
        if (!success) return NotFound();
        return NoContent();
    }
}
