using KPW.Application.DTOs.Ai;
using KPW.Application.Features.Ai.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KPW.Api.Controllers;

[ApiController]
[Route("ai")]
[Route("api/ai")]
[Authorize]
public class AiController : ControllerBase
{
    private readonly IMediator _mediator;

    public AiController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("chat")]
    public async Task<ActionResult<AiChatResponseDto>> Chat(
        [FromBody] AiChatRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new AiChatCommand(request), cancellationToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
