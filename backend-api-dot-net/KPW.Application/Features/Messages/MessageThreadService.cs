using KPW.Application.DTOs.Messages;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using KPW.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.Messages;

internal static class MessageMapper
{
    public static MessageDto ToDto(Message message, IFileStorageService? fileStorage = null)
    {
        string? videoTitle = null;
        if (message.VideoSubmission != null)
        {
            videoTitle = !string.IsNullOrWhiteSpace(message.VideoSubmission.Title)
                ? message.VideoSubmission.Title
                : !string.IsNullOrWhiteSpace(message.VideoSubmission.Exercise?.Title)
                    ? message.VideoSubmission.Exercise.Title
                    : !string.IsNullOrWhiteSpace(message.VideoSubmission.Notes)
                        ? message.VideoSubmission.Notes
                        : $"Video #{message.VideoSubmissionId}";
        }
        else if (message.VideoSubmissionId.HasValue)
        {
            videoTitle = $"Video #{message.VideoSubmissionId}";
        }

        var senderName = message.Sender != null
            ? $"{message.Sender.FirstName} {message.Sender.LastName}".Trim()
            : "User";

        return new(
            message.MessageId,
            message.MessageThreadId,
            message.SenderUserId,
            senderName,
            message.Body,
            message.VideoSubmissionId,
            videoTitle,
            ResolveMediaUrl(message.AttachmentUrl, fileStorage),
            message.AttachmentName,
            message.AttachmentType,
            message.ReadAt,
            message.CreatedDate,
            ResolveMediaUrl(message.Sender?.ProfilePictureUrl, fileStorage));
    }

    private static string? ResolveMediaUrl(string? path, IFileStorageService? fileStorage)
    {
        if (string.IsNullOrWhiteSpace(path)) return null;

        if (fileStorage != null)
        {
            return fileStorage.GetPermanentUrl(path);
        }

        var trimmed = path.Trim();
        if (trimmed.StartsWith("/api/media", StringComparison.OrdinalIgnoreCase))
        {
            return trimmed;
        }

        var normalized = trimmed;
        if (trimmed.Contains("storage.googleapis.com", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var uri = new Uri(trimmed);
                var absPath = uri.AbsolutePath.TrimStart('/');
                var slashIndex = absPath.IndexOf('/');
                normalized = slashIndex >= 0 ? absPath[(slashIndex + 1)..] : absPath;
            }
            catch
            {
                // keep path
            }
        }

        normalized = normalized.TrimStart('/', '\\');
        return $"/api/media/view?path={Uri.EscapeDataString(normalized)}";
    }
}

internal static class MessageThreadService
{
    public static async Task<(int OwnerId, int PhysioId)> ResolveParticipantsAsync(
        DbContext dbContext,
        ICurrentUserService currentUser,
        int petId,
        CancellationToken cancellationToken)
    {
        var existingThread = await dbContext.Set<MessageThread>()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.PetId == petId, cancellationToken);

        var pet = await dbContext.Set<Pet>()
            .Include(p => p.Owner)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PetId == petId, cancellationToken);

        if (pet is null)
        {
            throw new KeyNotFoundException("Pet not found.");
        }

        if (currentUser.Role is UserRole.Physio or UserRole.SysAdmin)
        {
            var physioId = currentUser.Role == UserRole.SysAdmin
                ? (existingThread?.PhysioId ?? await ResolvePhysioForPetAsync(dbContext, pet, cancellationToken))
                : currentUser.UserId!.Value;

            return (pet.OwnerId, physioId);
        }

        if (currentUser.Role == UserRole.Owner)
        {
            if (pet.OwnerId != currentUser.UserId)
            {
                throw new UnauthorizedAccessException("You can only message about your own pets.");
            }

            var physioId = existingThread?.PhysioId ?? await ResolvePhysioForPetAsync(dbContext, pet, cancellationToken);
            return (pet.OwnerId, physioId);
        }

        throw new UnauthorizedAccessException();
    }

    public static async Task<MessageThread> GetOrCreateThreadAsync(
        DbContext dbContext,
        int petId,
        int ownerId,
        int physioId,
        CancellationToken cancellationToken)
    {
        var existing = await dbContext.Set<MessageThread>()
            .FirstOrDefaultAsync(t => t.PetId == petId, cancellationToken);

        if (existing is not null)
        {
            return existing;
        }

        var thread = new MessageThread
        {
            PetId = petId,
            OwnerId = ownerId,
            PhysioId = physioId
        };

        dbContext.Set<MessageThread>().Add(thread);
        await dbContext.SaveChangesAsync(cancellationToken);
        return thread;
    }

    private static async Task<int> ResolvePhysioForPetAsync(
        DbContext dbContext,
        Pet pet,
        CancellationToken cancellationToken)
    {
        var activeProgram = await dbContext.Set<RehabProgram>()
            .Where(p => p.PetId == pet.PetId && p.IsActive)
            .OrderByDescending(p => p.StartDate)
            .FirstOrDefaultAsync(cancellationToken);

        if (activeProgram is not null)
        {
            return activeProgram.PhysioId;
        }

        var anyProgram = await dbContext.Set<RehabProgram>()
            .Where(p => p.PetId == pet.PetId)
            .OrderByDescending(p => p.StartDate)
            .FirstOrDefaultAsync(cancellationToken);

        if (anyProgram is not null)
        {
            return anyProgram.PhysioId;
        }

        var appointment = await dbContext.Set<Appointment>()
            .Where(a => a.PetId == pet.PetId)
            .OrderByDescending(a => a.ScheduledDateTime)
            .FirstOrDefaultAsync(cancellationToken);

        if (appointment is not null)
        {
            return appointment.PhysioId;
        }

        if (pet.Owner?.ClinicId is not null)
        {
            var clinicPhysio = await dbContext.Set<User>()
                .Where(u => u.ClinicId == pet.Owner.ClinicId && u.UserRole == UserRole.Physio && u.IsActive)
                .Select(u => (int?)u.UserId)
                .FirstOrDefaultAsync(cancellationToken);

            if (clinicPhysio.HasValue)
            {
                return clinicPhysio.Value;
            }
        }

        var fallbackPhysio = await dbContext.Set<User>()
            .Where(u => u.UserRole == UserRole.Physio && u.IsActive)
            .Select(u => (int?)u.UserId)
            .FirstOrDefaultAsync(cancellationToken);

        if (fallbackPhysio.HasValue)
        {
            return fallbackPhysio.Value;
        }

        throw new InvalidOperationException(
            "No physiotherapist assigned to this pet or clinic. Please contact support.");
    }
}
