using FluentValidation;
using KPW.Application.DTOs.Auth;
using KPW.Application.Features.Auth;
using KPW.Application.Features.Auth.Commands;
using KPW.Application.Features.Auth.Queries;
using KPW.Application.Features.Auth.Validators;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KPW.Api.Controllers;

[ApiController]
[Route("auth")]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("check-email")]
    [AllowAnonymous]
    public async Task<ActionResult<CheckEmailResponseDto>> CheckEmail(
        [FromQuery] string? email,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CheckEmailQuery(email), cancellationToken);
        return Ok(result);
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Register(
        [FromBody] RegisterRequestDto request,
        [FromServices] IValidator<RegisterRequestDto> validator,
        CancellationToken cancellationToken)
    {
        try
        {
            await validator.ValidateAndThrowAsync(request, cancellationToken);
            var result = await _mediator.Send(new RegisterCommand(request), cancellationToken);
            return Ok(result);
        }
        catch (ValidationException ex)
        {
            var firstError = ex.Errors.FirstOrDefault()?.ErrorMessage ?? "Validation failed.";
            return BadRequest(new
            {
                message = firstError,
                errors = ex.Errors.ToDictionary(e => e.PropertyName, e => new[] { e.ErrorMessage })
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Login(
        [FromBody] LoginRequestDto request,
        [FromServices] IValidator<LoginRequestDto> validator,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        try
        {
            var result = await _mediator.Send(new LoginCommand(request), cancellationToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Refresh(
        [FromBody] RefreshTokenRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new RefreshTokenCommand(request), cancellationToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<ActionResult<ForgotPasswordResponseDto>> ForgotPassword(
        [FromBody] ForgotPasswordRequestDto request,
        [FromServices] IValidator<ForgotPasswordRequestDto> validator,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        var result = await _mediator.Send(new ForgotPasswordCommand(request), cancellationToken);
        return Ok(result);
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<ActionResult<MessageResponseDto>> ResetPassword(
        [FromBody] ResetPasswordRequestDto request,
        [FromServices] IValidator<ResetPasswordRequestDto> validator,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        try
        {
            var result = await _mediator.Send(new ResetPasswordCommand(request), cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("change-password")]
    [Authorize]
    public async Task<ActionResult<MessageResponseDto>> ChangePassword(
        [FromBody] ChangePasswordRequestDto request,
        [FromServices] IValidator<ChangePasswordRequestDto> validator,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        try
        {
            var result = await _mediator.Send(new ChangePasswordCommand(request), cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<AuthUserDto>> Me(CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new GetCurrentUserQuery(), cancellationToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPut("profile")]
    [Authorize]
    public async Task<ActionResult<AuthUserDto>> UpdateProfile(
        [FromBody] UpdateProfileRequestDto request,
        [FromServices] IValidator<UpdateProfileRequestDto> validator,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        try
        {
            var result = await _mediator.Send(new UpdateProfileCommand(request), cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost("profile-picture")]
    [Authorize]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<AuthUserDto>> UploadProfilePicture(
        [FromForm] IFormFile file,
        [FromServices] IFileStorageService fileStorageService,
        [FromServices] DbContext dbContext,
        [FromServices] ICurrentUserService currentUserService,
        CancellationToken cancellationToken)
    {
        if (currentUserService.UserId is null)
        {
            return Unauthorized(new { message = "User is not authenticated." });
        }

        if (file is null || file.Length == 0)
        {
            return BadRequest(new { message = "No image selected." });
        }

        const long maxBytes = 5 * 1024 * 1024; // 5 MB
        if (file.Length > maxBytes)
        {
            return BadRequest(new { message = "Image size exceeds 5 MB limit." });
        }

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(ext))
        {
            return BadRequest(new { message = "Only JPG, PNG, and WebP images are allowed." });
        }

        var user = await dbContext.Set<User>()
            .FirstOrDefaultAsync(u => u.UserId == currentUserService.UserId, cancellationToken);

        if (user is null)
        {
            return Unauthorized(new { message = "User not found." });
        }

        await using var stream = file.OpenReadStream();
        var storagePath = await fileStorageService.UploadAsync(
            stream,
            file.FileName,
            folder: "avatars",
            contentType: file.ContentType,
            cancellationToken: cancellationToken);

        user.ProfilePictureUrl = fileStorageService.GetPublicUrl(storagePath);
        await dbContext.SaveChangesAsync(cancellationToken);

        var clinic = user.ClinicId is null
            ? null
            : await dbContext.Set<Clinic>()
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.ClinicId == user.ClinicId, cancellationToken);

        return Ok(AuthUserMapper.ToDto(user, clinic));
    }

    [HttpDelete("profile-picture")]
    [Authorize]
    public async Task<ActionResult<AuthUserDto>> DeleteProfilePicture(
        [FromServices] DbContext dbContext,
        [FromServices] ICurrentUserService currentUserService,
        CancellationToken cancellationToken)
    {
        if (currentUserService.UserId is null)
        {
            return Unauthorized(new { message = "User is not authenticated." });
        }

        var user = await dbContext.Set<User>()
            .FirstOrDefaultAsync(u => u.UserId == currentUserService.UserId, cancellationToken);

        if (user is null)
        {
            return Unauthorized(new { message = "User not found." });
        }

        user.ProfilePictureUrl = null;
        await dbContext.SaveChangesAsync(cancellationToken);

        var clinic = user.ClinicId is null
            ? null
            : await dbContext.Set<Clinic>()
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.ClinicId == user.ClinicId, cancellationToken);

        return Ok(AuthUserMapper.ToDto(user, clinic));
    }

    [HttpPost("verify-email")]
    [AllowAnonymous]
    public async Task<ActionResult<VerifyEmailResponseDto>> VerifyEmail(
        [FromBody] VerifyEmailRequestDto request,
        [FromServices] IValidator<VerifyEmailRequestDto> validator,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        try
        {
            var result = await _mediator.Send(new VerifyEmailCommand(request), cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("resend-verification")]
    [AllowAnonymous]
    public async Task<ActionResult<MessageResponseDto>> ResendVerification(
        [FromBody] ResendVerificationEmailRequestDto request,
        [FromServices] IValidator<ResendVerificationEmailRequestDto> validator,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        var result = await _mediator.Send(new ResendVerificationEmailCommand(request), cancellationToken);
        return Ok(result);
    }

    [HttpPost("send-owner-invite")]
    [Authorize]
    public async Task<ActionResult<MessageResponseDto>> SendOwnerInvite(
        [FromBody] SendOwnerInviteRequestDto request,
        [FromServices] IValidator<SendOwnerInviteRequestDto> validator,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        try
        {
            var result = await _mediator.Send(new SendOwnerInviteCommand(request), cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost("request-data-deletion")]
    [AllowAnonymous]
    public async Task<ActionResult<DataDeletionResponseDto>> RequestDataDeletion(
        [FromBody] DataDeletionRequestDto request,
        [FromServices] IValidator<DataDeletionRequestDto> validator,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        try
        {
            var result = await _mediator.Send(new RequestDataDeletionCommand(request), cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

