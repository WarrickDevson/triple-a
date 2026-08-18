using FluentValidation;
using KPW.Application.DTOs.Pets;
using KPW.Application.Features.Pets.Commands;
using KPW.Application.Features.Pets.Queries;
using KPW.Application.Features.Pets.Validators;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KPW.Api.Controllers;

[ApiController]
[Route("pets")]
[Route("api/pets")]
[Authorize]
public class PetsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PetsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<PetDto>> Create(
        [FromBody] CreatePetRequestDto request,
        [FromServices] IValidator<CreatePetRequestDto> validator,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        try
        {
            var result = await _mediator.Send(new CreatePetCommand(request), cancellationToken);
            return CreatedAtAction(nameof(GetByOwner), new { id = result.OwnerId }, result);
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

    [HttpGet("owner/{id:int}")]
    public async Task<ActionResult<IReadOnlyList<PetDto>>> GetByOwner(int id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new GetPetsByOwnerQuery(id), cancellationToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpGet("clinic")]
    public async Task<ActionResult<IReadOnlyList<PetDto>>> GetClinicPatients(CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new GetClinicPatientsQuery(), cancellationToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<PetDto>> Update(
        int id,
        [FromBody] UpdatePetRequestDto request,
        [FromServices] IValidator<UpdatePetRequestDto> validator,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        try
        {
            var result = await _mediator.Send(new UpdatePetCommand(id, request), cancellationToken);
            return Ok(result);
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
