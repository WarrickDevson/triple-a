using KPW.Application.DTOs.Progress;
using KPW.Application.Features.Progress.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KPW.Api.Controllers;

[ApiController]
[Route("pets/{petId:int}/progress")]
[Authorize]
public class PetProgressController : ControllerBase
{
    private readonly IMediator _mediator;

    public PetProgressController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<PetProgressSummaryDto>> Get(int petId, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new GetPetProgressQuery(petId), cancellationToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }
}
