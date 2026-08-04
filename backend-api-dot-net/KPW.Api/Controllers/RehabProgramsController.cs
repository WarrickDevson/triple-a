using FluentValidation;
using KPW.Application.DTOs.RehabPrograms;
using KPW.Application.Features.RehabPrograms.Commands;
using KPW.Application.Features.RehabPrograms.Queries;
using KPW.Application.Features.RehabPrograms.Validators;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KPW.Api.Controllers;

[ApiController]
[Route("rehab-programs")]
[Authorize]
public class RehabProgramsController : ControllerBase
{
    private readonly IMediator _mediator;

    public RehabProgramsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("pet/{petId:int}")]
    public async Task<ActionResult<IReadOnlyList<RehabProgramDto>>> GetByPet(int petId, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new GetRehabProgramsByPetQuery(petId), cancellationToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<RehabProgramDto>> Create(
        [FromBody] CreateRehabProgramRequestDto request,
        [FromServices] IValidator<CreateRehabProgramRequestDto> validator,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        try
        {
            var result = await _mediator.Send(new CreateRehabProgramCommand(request), cancellationToken);
            return CreatedAtAction(nameof(GetByPet), new { petId = result.PetId }, result);
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
