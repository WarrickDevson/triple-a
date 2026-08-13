using KPW.Application.DTOs.SoapNotes;
using KPW.Application.Features.SoapNotes.Commands;
using KPW.Application.Features.SoapNotes.Queries;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KPW.Api.Controllers;

[ApiController]
[Route("soap-notes")]
[Authorize]
public class SoapNotesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ISoapReportPdfGenerator _pdfGenerator;
    private readonly DbContext _dbContext;

    public SoapNotesController(
        IMediator mediator,
        ISoapReportPdfGenerator pdfGenerator,
        DbContext dbContext)
    {
        _mediator = mediator;
        _pdfGenerator = pdfGenerator;
        _dbContext = dbContext;
    }

    [HttpGet("pet/{petId:int}")]
    public async Task<ActionResult<IReadOnlyList<SoapNoteDto>>> GetByPet(int petId, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new GetSoapNotesByPetQuery(petId), cancellationToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost("pet/{petId:int}")]
    public async Task<ActionResult<SoapNoteDto>> Create(
        int petId,
        [FromBody] CreateSoapNoteRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new CreateSoapNoteCommand(petId, request), cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.SoapNoteId }, result);
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

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SoapNoteDto>> GetById(int id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new GetSoapNoteByIdQuery(id), cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<SoapNoteDto>> Update(
        int id,
        [FromBody] UpdateSoapNoteRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new UpdateSoapNoteCommand(id, request), cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var success = await _mediator.Send(new DeleteSoapNoteCommand(id), cancellationToken);
        if (!success) return NotFound();
        return NoContent();
    }

    [HttpGet("{id:int}/pdf")]
    public async Task<IActionResult> DownloadPdf(int id, CancellationToken cancellationToken)
    {
        try
        {
            var soapNote = await _mediator.Send(new GetSoapNoteByIdQuery(id), cancellationToken);
            var pet = await _dbContext.Set<Pet>()
                .Include(p => p.Owner)
                .FirstOrDefaultAsync(p => p.PetId == soapNote.PetId, cancellationToken);

            if (pet is null) return NotFound("Pet record not found.");

            var ownerName = $"{pet.Owner.FirstName} {pet.Owner.LastName}".Trim();
            var pdfBytes = _pdfGenerator.Generate(soapNote, pet.PetName, pet.Species, pet.Breed, ownerName);

            var fileName = $"{pet.PetName.Replace(" ", "_")}_SOAP_Report_{soapNote.SessionDate:yyyyMMdd}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("pet/{petId:int}/owner-notes")]
    public async Task<ActionResult<OwnerSubjectiveNoteDto>> CreateOwnerNote(
        int petId,
        [FromBody] CreateOwnerSubjectiveNoteRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new CreateOwnerSubjectiveNoteCommand(petId, request), cancellationToken);
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

    [HttpGet("pet/{petId:int}/owner-notes")]
    public async Task<ActionResult<IReadOnlyList<OwnerSubjectiveNoteDto>>> GetOwnerNotes(int petId, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new GetOwnerSubjectiveNotesQuery(petId), cancellationToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }
}
