using KPW.Application.DTOs.Dashboard;
using KPW.Application.Features.Dashboard.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KPW.Api.Controllers;

[ApiController]
[Route("dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("physio")]
    public async Task<ActionResult<PhysioDashboardDto>> GetPhysioDashboard(CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new GetPhysioDashboardQuery(), cancellationToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }
}
