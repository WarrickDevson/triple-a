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
[Route("pets/{petId:int}/messages")]
[Authorize]
public class PetMessagesController : ControllerBase
{
    private readonly IMediator _mediator;

    public PetMessagesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MessageDto>>> GetAll(
        int petId,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new GetPetMessagesQuery(petId), cancellationToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<MessageDto>> Send(
        int petId,
        [FromBody] SendMessageRequestDto request,
        [FromServices] IValidator<SendMessageRequestDto> validator,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        try
        {
            var result = await _mediator.Send(new SendMessageCommand(petId, request), cancellationToken);
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
