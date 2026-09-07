using KPW.Application.DTOs.Ai;
using KPW.Application.Features.Ai.Commands;
using KPW.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KPW.Api.Controllers;

[ApiController]
[Route("ai")]
[Route("api/ai")]
[Authorize]
public class AiController : ControllerBase
{
    private readonly IMediator _mediator;

    public AiController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("chat")]
    public async Task<ActionResult<AiChatResponseDto>> Chat(
        [FromBody] AiChatRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new AiChatCommand(request), cancellationToken);
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

    [HttpGet("prompt-config")]
    public async Task<ActionResult<AiPromptConfigDto>> GetPromptConfig(
        [FromServices] IAiPromptConfigService promptConfigService,
        CancellationToken cancellationToken)
    {
        var config = await promptConfigService.GetPromptConfigAsync(cancellationToken);
        return Ok(config);
    }

    [HttpPut("prompt-config")]
    [Authorize(Roles = "SysAdmin,ClinicAdmin,Physiotherapist")]
    public async Task<ActionResult<AiPromptConfigDto>> UpdatePromptConfig(
        [FromBody] UpdateAiPromptConfigRequestDto request,
        [FromServices] IAiPromptConfigService promptConfigService,
        [FromServices] ICurrentUserService currentUserService,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.SystemPrompt))
        {
            return BadRequest(new { message = "System prompt cannot be empty." });
        }

        var updatedBy = currentUserService.Email ?? currentUserService.UserId?.ToString() ?? "Admin";
        var result = await promptConfigService.UpdatePromptConfigAsync(request.SystemPrompt, updatedBy, cancellationToken);
        return Ok(result);
    }

    [HttpPost("prompt-config/reset")]
    [Authorize(Roles = "SysAdmin,ClinicAdmin,Physiotherapist")]
    public async Task<ActionResult<AiPromptConfigDto>> ResetPromptConfig(
        [FromServices] IAiPromptConfigService promptConfigService,
        [FromServices] ICurrentUserService currentUserService,
        CancellationToken cancellationToken)
    {
        var updatedBy = currentUserService.Email ?? currentUserService.UserId?.ToString() ?? "Admin";
        var result = await promptConfigService.ResetPromptConfigAsync(updatedBy, cancellationToken);
        return Ok(result);
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
            folder: "ai-attachments",
            contentType: file.ContentType,
            cancellationToken: cancellationToken);

        var attachmentUrl = fileStorageService.GetPublicUrl(storagePath);
        return Ok(new
        {
            attachmentUrl,
            attachmentName = file.FileName,
            attachmentType = file.ContentType
        });
    }
}
