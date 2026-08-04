using KPW.Application.DTOs.Exercises;
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
}
