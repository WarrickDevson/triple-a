using KPW.Application.DTOs.Videos;
using KPW.Application.Features.Videos.Commands;
using KPW.Application.Features.Videos.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KPW.Api.Controllers;

[ApiController]
[Route("pets/{petId:int}/videos")]
[Route("api/pets/{petId:int}/videos")]
[Authorize]
public class PetVideosController : ControllerBase
{
    private readonly IMediator _mediator;

    public PetVideosController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<VideoSubmissionDto>>> GetAll(
        int petId,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new GetPetVideosQuery(petId), cancellationToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost]
    [RequestSizeLimit(104_857_600)]
    [RequestFormLimits(MultipartBodyLengthLimit = 104_857_600)]
    public async Task<ActionResult<UploadVideoResultDto>> Upload(
        int petId,
        [FromForm] int? exerciseId,
        [FromForm] string? title,
        [FromForm] string? notes,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { message = "Video file is required." });
        }

        try
        {
            await using var stream = file.OpenReadStream();
            var result = await _mediator.Send(
                new UploadVideoCommand(petId, exerciseId, title, notes, stream, file.FileName, file.ContentType, file.Length),
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
