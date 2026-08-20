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

    [HttpGet("pet/{petId:int}/download")]
    public async Task<IActionResult> DownloadPetReport(int petId, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new GeneratePetReportQuery(petId), cancellationToken);
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

    [HttpGet("pet/{petId:int}/shared")]
    public async Task<ActionResult<IReadOnlyList<KPW.Application.DTOs.SoapNotes.SharedReportDto>>> GetSharedReports(int petId, CancellationToken cancellationToken)
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
    public async Task<ActionResult<KPW.Application.DTOs.SoapNotes.SharedReportDto>> ShareDocument(
        int petId,
        [FromBody] KPW.Application.DTOs.SoapNotes.ShareDocumentRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new KPW.Application.Features.Reports.Commands.ShareDocumentCommand(petId, request), cancellationToken);
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
    public async Task<ActionResult<KPW.Application.DTOs.SoapNotes.SharedReportDto>> PublishProgressReport(
        int petId,
        [FromQuery] string? title,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new KPW.Application.Features.Reports.Commands.PublishProgressReportCommand(petId, title), cancellationToken);
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
    public async Task<IActionResult> DeleteSharedReport(int sharedReportId, CancellationToken cancellationToken)
    {
        var success = await _mediator.Send(new KPW.Application.Features.Reports.Commands.DeleteSharedReportCommand(sharedReportId), cancellationToken);
        if (!success) return NotFound();
        return NoContent();
    }
}
