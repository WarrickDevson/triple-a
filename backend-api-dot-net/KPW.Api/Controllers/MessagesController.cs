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
    public async Task<ActionResult> UploadAttachment(IFormFile file, [FromServices] IWebHostEnvironment env)
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

        var wwwrootFolder = Path.Combine(env.ContentRootPath, "wwwroot", "uploads", "attachments");
        var directFolder = Path.Combine(env.ContentRootPath, "uploads", "attachments");
        Directory.CreateDirectory(wwwrootFolder);
        Directory.CreateDirectory(directFolder);

        var ext = Path.GetExtension(file.FileName);
        var uniqueFileName = $"{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(wwwrootFolder, uniqueFileName);
        var secondaryPath = Path.Combine(directFolder, uniqueFileName);

        using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }
        System.IO.File.Copy(fullPath, secondaryPath, true);

        var attachmentUrl = $"/uploads/attachments/{uniqueFileName}";
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
