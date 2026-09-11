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
[Route("api/exercises")]
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

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ExerciseDto>> Update(
        int id,
        [FromBody] UpdateExerciseRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new UpdateExerciseCommand(id, request), cancellationToken);
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
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:int}/customize")]
    public async Task<ActionResult<ExerciseDto>> Customize(
        int id,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new CustomizeExerciseCommand(id), cancellationToken);
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
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:int}/toggle-active")]
    public async Task<ActionResult<ToggleActiveExerciseResult>> ToggleActive(
        int id,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new ToggleActiveExerciseCommand(id), cancellationToken);
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
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        try
        {
            var success = await _mediator.Send(new DeleteExerciseCommand(id), cancellationToken);
            if (!success)
            {
                return NotFound(new { message = "Exercise not found." });
            }
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost("upload-media")]
    [RequestSizeLimit(104_857_600)] // 100 MB max for video
    [RequestFormLimits(MultipartBodyLengthLimit = 104_857_600)]
    public async Task<ActionResult<UploadExerciseMediaResultDto>> UploadMedia(
        IFormFile file,
        [FromServices] KPW.Application.Interfaces.IFileStorageService fileStorageService,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { message = "No media file provided." });
        }

        var contentType = file.ContentType.ToLowerInvariant();
        var isVideo = contentType.StartsWith("video/") ||
                      file.FileName.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) ||
                      file.FileName.EndsWith(".mov", StringComparison.OrdinalIgnoreCase) ||
                      file.FileName.EndsWith(".webm", StringComparison.OrdinalIgnoreCase);

        var isImage = contentType.StartsWith("image/") ||
                      file.FileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                      file.FileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                      file.FileName.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                      file.FileName.EndsWith(".webp", StringComparison.OrdinalIgnoreCase) ||
                      file.FileName.EndsWith(".gif", StringComparison.OrdinalIgnoreCase);

        if (!isVideo && !isImage)
        {
            return BadRequest(new { message = "Only video (mp4, mov, webm) or image (png, jpg, webp) files are accepted." });
        }

        var folder = isVideo ? "exercise-videos" : "exercise-images";
        await using var stream = file.OpenReadStream();
        var storagePath = await fileStorageService.UploadAsync(
            stream,
            file.FileName,
            folder: folder,
            contentType: file.ContentType,
            cancellationToken: cancellationToken);

        var publicUrl = fileStorageService.GetPublicUrl(storagePath);

        return Ok(new UploadExerciseMediaResultDto(
            publicUrl,
            file.FileName,
            file.ContentType,
            isVideo));
    }
}

