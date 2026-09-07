using System.Text;
using KPW.Application.DTOs.Ai;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.Ai.Commands;

public record AiChatCommand(AiChatRequestDto Request) : IRequest<AiChatResponseDto>;

public class AiChatCommandHandler : IRequestHandler<AiChatCommand, AiChatResponseDto>
{
    private readonly IAiChatService _aiChatService;
    private readonly ICurrentUserService _currentUserService;
    private readonly DbContext _dbContext;

    public AiChatCommandHandler(
        IAiChatService aiChatService,
        ICurrentUserService currentUserService,
        DbContext dbContext)
    {
        _aiChatService = aiChatService;
        _currentUserService = currentUserService;
        _dbContext = dbContext;
    }

    public async Task<AiChatResponseDto> Handle(AiChatCommand command, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
        {
            throw new UnauthorizedAccessException();
        }

        var message = command.Request.Message?.Trim();
        var hasAttachment = !string.IsNullOrWhiteSpace(command.Request.AttachmentBase64) ||
                            !string.IsNullOrWhiteSpace(command.Request.AttachmentUrl);

        if (string.IsNullOrWhiteSpace(message) && !hasAttachment)
        {
            throw new InvalidOperationException("Message or attachment is required.");
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            message = "Please analyze the attached image/document and advise from a veterinary physiotherapy and recovery perspective.";
        }

        AiChatAttachment? attachment = null;
        if (hasAttachment)
        {
            attachment = new AiChatAttachment(
                command.Request.AttachmentUrl,
                command.Request.AttachmentBase64,
                command.Request.AttachmentMimeType,
                command.Request.AttachmentName);
        }

        string? clinicalContext = null;

        // If the pet owner granted explicit permission to share their pet data:
        if (command.Request.IncludePetContext && _currentUserService.UserId.HasValue)
        {
            clinicalContext = await BuildPetClinicalContextAsync(
                _currentUserService.UserId.Value,
                command.Request.PetId,
                cancellationToken);
        }

        var historyTurns = command.Request.History?
            .Select(h => new AiChatHistoryTurn(h.Role, h.Content))
            .ToList();

        var result = await _aiChatService.ChatAsync(message, clinicalContext, attachment, historyTurns, cancellationToken);

        return new AiChatResponseDto(
            result.Answer,
            result.Sources.Select(s => new AiChatSourceDto(s.Title, s.Excerpt)).ToList());
    }

    private async Task<string?> BuildPetClinicalContextAsync(
        int ownerId,
        int? specificPetId,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Set<Pet>()
            .Where(p => p.OwnerId == ownerId)
            .Include(p => p.MedicalHistories)
            .Include(p => p.RehabPrograms)
                .ThenInclude(rp => rp.RehabProgramExercises)
                    .ThenInclude(rpe => rpe.Exercise)
            .AsNoTracking();

        if (specificPetId.HasValue && specificPetId.Value > 0)
        {
            query = query.Where(p => p.PetId == specificPetId.Value);
        }

        var pets = await query.ToListAsync(cancellationToken);
        if (pets.Count == 0)
        {
            return null;
        }

        var sb = new StringBuilder();
        sb.AppendLine($"Verified Clinical Information for ALL Registered Pets ({pets.Count} companion{(pets.Count > 1 ? "s" : "")}):");

        int index = 1;
        foreach (var pet in pets)
        {
            var ageText = pet.BirthDate.HasValue
                ? $"Born {pet.BirthDate.Value:yyyy-MM-dd}"
                : "Age unknown";
            var weightText = pet.WeightKg.HasValue ? $"{pet.WeightKg.Value} kg" : "Weight not logged";
            sb.AppendLine();
            sb.AppendLine($"--- [PET {index++} of {pets.Count}]: {pet.PetName} (Species: {pet.Species}, Breed: {pet.Breed ?? "Unspecified"}, {ageText}, {weightText}) ---");

            if (pet.MedicalHistories.Count > 0)
            {
                sb.AppendLine("  Clinical Diagnoses & History:");
                foreach (var mh in pet.MedicalHistories)
                {
                    var surgery = mh.SurgeryDate.HasValue ? $" (Surgery Date: {mh.SurgeryDate.Value:yyyy-MM-dd})" : string.Empty;
                    var detail = !string.IsNullOrWhiteSpace(mh.InjuryOrCondition) ? $" - {mh.InjuryOrCondition}" : string.Empty;
                    var notes = !string.IsNullOrWhiteSpace(mh.ClinicianNotes) ? $" [Notes: {mh.ClinicianNotes}]" : string.Empty;
                    sb.AppendLine($"   * {mh.Diagnosis}{detail}{surgery}{notes}");
                }
            }

            var activePrograms = pet.RehabPrograms.Where(rp => rp.EndDate == null || rp.EndDate >= DateOnly.FromDateTime(DateTime.UtcNow)).ToList();
            if (activePrograms.Count > 0)
            {
                sb.AppendLine("  Current Rehabilitation Plan(s):");
                foreach (var prog in activePrograms)
                {
                    sb.AppendLine($"   * Plan: {prog.ProgramTitle} (Started: {prog.StartDate:yyyy-MM-dd}{(prog.Notes != null ? $" | Notes: {prog.Notes}" : "")})");
                    if (prog.RehabProgramExercises.Count > 0)
                    {
                        sb.AppendLine("     Prescribed Rehab Exercises:");
                        foreach (var pe in prog.RehabProgramExercises)
                        {
                            var ex = pe.Exercise;
                            var title = ex?.Title ?? "Exercise";
                            var purpose = !string.IsNullOrWhiteSpace(ex?.ClinicalPurpose) ? $" | Purpose: {ex.ClinicalPurpose}" : string.Empty;
                            var safety = !string.IsNullOrWhiteSpace(ex?.SafetyNotes) ? $" | Safety: {ex.SafetyNotes}" : string.Empty;
                            sb.AppendLine($"       - {title}: {pe.Sets} sets x {pe.Repetitions} reps ({pe.FrequencyPerDay}x/day){purpose}{safety}");
                        }
                    }
                }
            }
        }

        return sb.ToString().TrimEnd();
    }
}
