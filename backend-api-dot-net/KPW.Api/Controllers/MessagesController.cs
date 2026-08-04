using FluentValidation;
using KPW.Application.DTOs.Messages;
using KPW.Application.Features.Messages.Commands;
using KPW.Application.Features.Messages.Queries;
using KPW.Application.Features.Messages.Validators;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KPW.Api.Controllers;

[ApiController]
[Route("messages")]
[Authorize]
public class MessagesController : ControllerBase
{
    private readonly IMediator _mediator;

    public MessagesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("threads")]
    public async Task<ActionResult<IReadOnlyList<MessageThreadDto>>> GetThreads(
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new GetMessageThreadsQuery(), cancellationToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}/read")]
    public async Task<ActionResult<MessageDto>> MarkRead(int id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new MarkMessageReadCommand(id), cancellationToken);
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
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
