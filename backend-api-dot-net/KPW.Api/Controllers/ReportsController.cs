using KPW.Application.Features.Reports.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KPW.Api.Controllers;

[ApiController]
[Route("reports")]
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
}
