using FluentValidation;
using KPW.Application.DTOs.Pets;
using KPW.Application.Features.Pets;
using KPW.Application.Features.Pets.Commands;
using KPW.Application.Features.Pets.Queries;
using KPW.Application.Features.Pets.Validators;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

    [HttpPost("{id:int}/photo")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<PetDto>> UploadPhoto(
        int id,
        [FromForm] IFormFile file,
        [FromServices] IFileStorageService fileStorageService,
        [FromServices] DbContext dbContext,
        [FromServices] ICurrentUserService currentUserService,
        CancellationToken cancellationToken)
    {
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

        try
        {
            await PetAuthorization.EnsureCanAccessPet(dbContext, currentUserService, id, cancellationToken);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }

        var pet = await dbContext.Set<Pet>()
            .Include(p => p.Owner)
            .Include(p => p.MedicalHistories)
            .FirstOrDefaultAsync(p => p.PetId == id, cancellationToken);

        if (pet is null)
        {
            return NotFound(new { message = "Pet not found." });
        }

        await using var stream = file.OpenReadStream();
        var storagePath = await fileStorageService.UploadAsync(
            stream,
            file.FileName,
            folder: "pets",
            contentType: file.ContentType,
            cancellationToken: cancellationToken);

        pet.ProfilePictureUrl = fileStorageService.GetPublicUrl(storagePath);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(PetMapper.ToDto(pet));
    }

    [HttpDelete("{id:int}/photo")]
    public async Task<ActionResult<PetDto>> DeletePhoto(
        int id,
        [FromServices] DbContext dbContext,
        [FromServices] ICurrentUserService currentUserService,
        CancellationToken cancellationToken)
    {
        try
        {
            await PetAuthorization.EnsureCanAccessPet(dbContext, currentUserService, id, cancellationToken);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }

        var pet = await dbContext.Set<Pet>()
            .Include(p => p.Owner)
            .Include(p => p.MedicalHistories)
            .FirstOrDefaultAsync(p => p.PetId == id, cancellationToken);

        if (pet is null)
        {
            return NotFound(new { message = "Pet not found." });
        }

        pet.ProfilePictureUrl = null;
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(PetMapper.ToDto(pet));
    }
}
