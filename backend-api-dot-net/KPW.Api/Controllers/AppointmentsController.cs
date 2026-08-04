using FluentValidation;
using KPW.Application.DTOs.Appointments;
using KPW.Application.Features.Appointments.Commands;
using KPW.Application.Features.Appointments.Queries;
using KPW.Application.Features.Appointments.Validators;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KPW.Api.Controllers;

[ApiController]
[Route("appointments")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AppointmentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AppointmentDto>>> GetAll(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new GetAppointmentsQuery(from, to), cancellationToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<AppointmentDto>> Create(
        [FromBody] CreateAppointmentRequestDto request,
        [FromServices] IValidator<CreateAppointmentRequestDto> validator,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        try
        {
            var result = await _mediator.Send(new CreateAppointmentCommand(request), cancellationToken);
            return CreatedAtAction(nameof(GetAll), result);
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

    [HttpPut("{id:int}/status")]
    public async Task<ActionResult<AppointmentDto>> UpdateStatus(
        int id,
        [FromBody] UpdateAppointmentStatusRequestDto request,
        [FromServices] IValidator<UpdateAppointmentStatusRequestDto> validator,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        try
        {
            var result = await _mediator.Send(
                new UpdateAppointmentStatusCommand(id, request),
                cancellationToken);
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
