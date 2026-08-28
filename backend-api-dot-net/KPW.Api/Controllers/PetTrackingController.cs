using FluentValidation;
using KPW.Application.DTOs.Tracking;
using KPW.Application.Features.Tracking.Commands;
using KPW.Application.Features.Tracking.Queries;
using KPW.Application.Features.Tracking.Validators;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KPW.Api.Controllers;

[ApiController]
[Route("pets/{petId:int}/tracking")]
[Route("api/pets/{petId:int}/tracking")]
[Authorize]
public class PetTrackingController : ControllerBase
{
    private readonly IMediator _mediator;

    public PetTrackingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<DailyTrackingLogDto>> Upsert(
        int petId,
        [FromBody] UpsertTrackingRequestDto request,
        [FromServices] IValidator<UpsertTrackingRequestDto> validator,
        CancellationToken cancellationToken)
    {
        try
        {
            await validator.ValidateAndThrowAsync(request, cancellationToken);
            var result = await _mediator.Send(new UpsertTrackingCommand(petId, request), cancellationToken);
            return Ok(result);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = string.Join("; ", ex.Errors.Select(e => e.ErrorMessage)) });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DailyTrackingLogDto>>> Get(
        int petId,
        [FromQuery] int days = 14,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _mediator.Send(new GetTrackingLogsQuery(petId, days), cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
    }
}
