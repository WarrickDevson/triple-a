using KPW.Application.DTOs.Reminders;
using KPW.Application.Features.Reminders.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KPW.Api.Controllers;

[ApiController]
[Route("reminders")]
[Authorize]
public class RemindersController : ControllerBase
{
    private readonly IMediator _mediator;

    public RemindersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ReminderDto>>> GetAll(CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new GetRemindersQuery(), cancellationToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }
}
