using FluentValidation;
using KPW.Application.DTOs.Messages;
using KPW.Application.Features.Messages.Commands;
using KPW.Application.Features.Messages.Queries;
using KPW.Application.Features.Messages.Validators;
using KPW.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KPW.Api.Controllers;

[ApiController]
[Route("messages")]
[Route("api/messages")]
[Authorize]
public class MessagesController : ControllerBase
{
    private readonly IMediator _mediator;

    public MessagesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("threads")]
    public async Task<ActionResult<IReadOnlyList<MessageThreadDto>>> GetThreads(
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new GetMessageThreadsQuery(), cancellationToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}/read")]
    public async Task<ActionResult<MessageDto>> MarkRead(int id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new MarkMessageReadCommand(id), cancellationToken);
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

    [HttpPost("attachments/upload")]
    public async Task<ActionResult> UploadAttachment(
        IFormFile file,
        [FromServices] IFileStorageService fileStorageService,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { message = "No file uploaded." });
        }

        const long maxBytes = 25 * 1024 * 1024; // 25 MB limit
        if (file.Length > maxBytes)
        {
            return BadRequest(new { message = "File size exceeds 25 MB limit." });
        }

        await using var stream = file.OpenReadStream();
        var storagePath = await fileStorageService.UploadAsync(
            stream,
            file.FileName,
            folder: "attachments",
            contentType: file.ContentType,
            cancellationToken: cancellationToken);

        var attachmentUrl = fileStorageService.GetPublicUrl(storagePath);
        var attachmentName = file.FileName;
        var attachmentType = file.ContentType;

        return Ok(new
        {
            attachmentUrl,
            attachmentName,
            attachmentType
        });
    }
}
