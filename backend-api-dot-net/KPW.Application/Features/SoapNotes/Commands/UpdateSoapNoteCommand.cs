using System.Text.Json;
using KPW.Application.DTOs.SoapNotes;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.SoapNotes.Commands;

public record UpdateSoapNoteCommand(int SoapNoteId, UpdateSoapNoteRequestDto Request) : IRequest<SoapNoteDto>;

public class UpdateSoapNoteCommandHandler : IRequestHandler<UpdateSoapNoteCommand, SoapNoteDto>
{
    private readonly DbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public UpdateSoapNoteCommandHandler(DbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<SoapNoteDto> Handle(UpdateSoapNoteCommand command, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var note = await _dbContext.Set<SoapNote>()
            .Include(s => s.Physio)
            .FirstOrDefaultAsync(s => s.SoapNoteId == command.SoapNoteId, cancellationToken);

        if (note is null)
        {
            throw new KeyNotFoundException($"SOAP note with ID {command.SoapNoteId} not found.");
        }

        var req = command.Request;
        if (req.SessionDate.HasValue) note.SessionDate = req.SessionDate.Value;
        note.Subjective = req.Subjective ?? note.Subjective;
        note.Objective = req.Objective ?? note.Objective;
        note.Action = req.Action ?? note.Action;
        note.Plan = req.Plan ?? note.Plan;

        note.StiffnessScore = req.StiffnessScore;
        note.PainScore = req.PainScore;
        note.LamenessScore = req.LamenessScore;

        if (req.CustomMetrics is not null)
        {
            note.CustomMetricsJson = req.CustomMetrics.Count > 0
                ? JsonSerializer.Serialize(req.CustomMetrics)
                : null;
        }

        if (req.ShareWithOwner && !note.IsSharedWithOwner)
        {
            note.IsSharedWithOwner = true;
            note.SharedAtUtc = DateTime.UtcNow;

            var sharedReport = new SharedReport
            {
                PetId = note.PetId,
                SoapNoteId = note.SoapNoteId,
                SharedByPhysioId = _currentUserService.UserId.Value,
                Title = $"SOAP Session Report - {note.SessionDate:MMM dd, yyyy}",
                ReportType = "SOAP_SESSION",
                Summary = !string.IsNullOrWhiteSpace(note.Plan) ? note.Plan : note.Subjective,
                SharedAtUtc = DateTime.UtcNow
            };
            _dbContext.Set<SharedReport>().Add(sharedReport);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return SoapNoteMapper.ToDto(note);
    }
}
