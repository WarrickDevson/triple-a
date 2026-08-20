using System.Text.Json;
using KPW.Application.DTOs.SoapNotes;
using KPW.Application.Features.Pets;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using KPW.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.SoapNotes.Commands;

public record CreateSoapNoteCommand(int PetId, CreateSoapNoteRequestDto Request) : IRequest<SoapNoteDto>;

public class CreateSoapNoteCommandHandler : IRequestHandler<CreateSoapNoteCommand, SoapNoteDto>
{
    private readonly DbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public CreateSoapNoteCommandHandler(DbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<SoapNoteDto> Handle(CreateSoapNoteCommand command, CancellationToken cancellationToken)
    {
        if (_currentUserService.Role is not (UserRole.Physio or UserRole.SysAdmin))
        {
            throw new UnauthorizedAccessException("Only physiotherapists can record clinical assessment notes.");
        }

        await PetAuthorization.EnsureCanAccessPet(_dbContext, _currentUserService, command.PetId, cancellationToken);

        var pet = await _dbContext.Set<Pet>()
            .FirstOrDefaultAsync(p => p.PetId == command.PetId, cancellationToken);

        if (pet is null)
        {
            throw new KeyNotFoundException($"Pet with ID {command.PetId} not found.");
        }

        var req = command.Request;
        bool hasMetrics = req.CustomMetrics is { Count: > 0 };
        bool hasAudio = !string.IsNullOrWhiteSpace(req.AudioUrl);
        bool hasTranscript = !string.IsNullOrWhiteSpace(req.RawTranscript);

        string? metricsJson = null;
        if (hasMetrics || hasAudio || hasTranscript)
        {
            metricsJson = JsonSerializer.Serialize(new
            {
                metrics = req.CustomMetrics ?? [],
                audioUrl = req.AudioUrl,
                rawTranscript = req.RawTranscript
            });
        }

        var note = new SoapNote
        {
            PetId = command.PetId,
            PhysioId = _currentUserService.UserId!.Value,
            AppointmentId = req.AppointmentId,
            SessionDate = req.SessionDate ?? DateTime.UtcNow,
            Subjective = req.Subjective ?? string.Empty,
            Objective = req.Objective ?? string.Empty,
            Action = req.Action ?? string.Empty,
            Plan = req.Plan ?? string.Empty,
            StiffnessScore = req.StiffnessScore,
            PainScore = req.PainScore,
            LamenessScore = req.LamenessScore,
            CustomMetricsJson = metricsJson,
            IsSharedWithOwner = req.ShareWithOwner,
            SharedAtUtc = req.ShareWithOwner ? DateTime.UtcNow : null
        };

        _dbContext.Set<SoapNote>().Add(note);

        // Synchronize Diagnosis update to MedicalHistory if provided
        if (!string.IsNullOrWhiteSpace(req.DiagnosisUpdate))
        {
            var medHistory = new MedicalHistory
            {
                PetId = command.PetId,
                Diagnosis = req.DiagnosisUpdate.Trim(),
                ClinicianNotes = $"Updated via SOAP Note on {note.SessionDate:yyyy-MM-dd}"
            };
            _dbContext.Set<MedicalHistory>().Add(medHistory);
        }

        // If shared with owner, automatically publish a SharedReport entry
        if (req.ShareWithOwner)
        {
            var sharedReport = new SharedReport
            {
                PetId = command.PetId,
                SoapNote = note,
                SharedByPhysioId = _currentUserService.UserId!.Value,
                Title = $"SOAP Session Report - {note.SessionDate:MMM dd, yyyy}",
                ReportType = "SOAP_SESSION",
                Summary = !string.IsNullOrWhiteSpace(note.Plan) ? note.Plan : note.Subjective,
                SharedAtUtc = DateTime.UtcNow
            };
            _dbContext.Set<SharedReport>().Add(sharedReport);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var created = await _dbContext.Set<SoapNote>()
            .Include(s => s.Physio)
            .FirstAsync(s => s.SoapNoteId == note.SoapNoteId, cancellationToken);

        return SoapNoteMapper.ToDto(created);
    }
}
