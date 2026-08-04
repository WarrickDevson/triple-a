using FluentValidation;
using KPW.Application.DTOs.RehabPrograms;
using KPW.Application.Features.RehabPrograms.Commands;
using KPW.Application.Features.RehabPrograms.Validators;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KPW.Api.Controllers;

[ApiController]
[Route("pets/{petId:int}/exercise-sessions")]
[Authorize]
public class ExerciseSessionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ExerciseSessionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Complete(
        int petId,
        [FromBody] CompleteExerciseSessionRequestDto request,
        [FromServices] IValidator<CompleteExerciseSessionRequestDto> validator,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        try
        {
            await _mediator.Send(new CompleteExerciseSessionCommand(petId, request), cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }
}
