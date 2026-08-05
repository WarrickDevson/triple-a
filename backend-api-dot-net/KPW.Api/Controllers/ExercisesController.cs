using FluentValidation;
using KPW.Application.DTOs.Exercises;
using KPW.Application.Features.Exercises.Commands;
using KPW.Application.Features.Exercises.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KPW.Api.Controllers;

[ApiController]
[Route("exercises")]
[Authorize]
public class ExercisesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ExercisesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ExerciseDto>>> Get(
        [FromQuery] string? species,
        [FromQuery] string? condition,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetExercisesQuery(species, condition), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ExerciseDto>> Create(
        [FromBody] CreateExerciseRequestDto request,
        [FromServices] IValidator<CreateExerciseRequestDto> validator,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        try
        {
            var result = await _mediator.Send(new CreateExerciseCommand(request), cancellationToken);
            return CreatedAtAction(nameof(Get), new { id = result.ExerciseId }, result);
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
