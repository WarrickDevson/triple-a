using KPW.Application.DTOs.Messages;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using KPW.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.Messages;

internal static class MessageMapper
{
    public static MessageDto ToDto(Message message) =>
        new(
            message.MessageId,
            message.MessageThreadId,
            message.SenderUserId,
            $"{message.Sender.FirstName} {message.Sender.LastName}",
            message.Body,
            message.VideoSubmissionId,
            message.ReadAt,
            message.CreatedDate);
}

internal static class MessageThreadService
{
    public static async Task<(int OwnerId, int PhysioId)> ResolveParticipantsAsync(
        DbContext dbContext,
        ICurrentUserService currentUser,
        int petId,
        CancellationToken cancellationToken)
    {
        var pet = await dbContext.Set<Pet>()
            .AsNoTracking()
            .FirstAsync(p => p.PetId == petId, cancellationToken);

        if (currentUser.Role is UserRole.Physio or UserRole.SysAdmin)
        {
            var physioId = currentUser.Role == UserRole.SysAdmin
                ? await ResolvePhysioForPetAsync(dbContext, petId, cancellationToken)
                : currentUser.UserId!.Value;

            return (pet.OwnerId, physioId);
        }

        if (currentUser.Role == UserRole.Owner)
        {
            if (pet.OwnerId != currentUser.UserId)
            {
                throw new UnauthorizedAccessException("You can only message about your own pets.");
            }

            var physioId = await ResolvePhysioForPetAsync(dbContext, petId, cancellationToken);
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
        int petId,
        CancellationToken cancellationToken)
    {
        var activeProgram = await dbContext.Set<RehabProgram>()
            .Where(p => p.PetId == petId && p.IsActive)
            .OrderByDescending(p => p.StartDate)
            .FirstOrDefaultAsync(cancellationToken);

        if (activeProgram is null)
        {
            throw new InvalidOperationException(
                "No active rehabilitation programme found for this pet. Please contact your physiotherapist.");
        }

        return activeProgram.PhysioId;
    }
}
