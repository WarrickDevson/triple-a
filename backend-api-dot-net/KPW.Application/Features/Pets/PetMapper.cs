using KPW.Application.DTOs.Pets;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using KPW.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.Pets;

internal static class PetMapper
{
    public static PetDto ToDto(Pet pet) =>
        new(
            pet.PetId,
            pet.OwnerId,
            $"{pet.Owner.FirstName} {pet.Owner.LastName}",
            pet.PetName,
            pet.Species,
            pet.Breed,
            pet.BirthDate,
            pet.WeightKg,
            pet.MedicalHistories.Select(m => new MedicalHistoryDto(
                m.MedicalHistoryId,
                m.Diagnosis,
                m.InjuryOrCondition,
                m.SurgeryDate,
                m.ClinicianNotes)).ToList());
}

internal static class PetAuthorization
{
    public static void EnsureCanAccessOwner(ICurrentUserService currentUser, int ownerId)
    {
        if (currentUser.UserId is null)
        {
            throw new UnauthorizedAccessException();
        }

        if (currentUser.Role is UserRole.SysAdmin or UserRole.Physio)
        {
            return;
        }

        if (currentUser.Role == UserRole.Owner && currentUser.UserId != ownerId)
        {
            throw new UnauthorizedAccessException("You can only access your own pets.");
        }
    }

    public static async Task EnsureCanAccessPet(
        DbContext dbContext,
        ICurrentUserService currentUser,
        int petId,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null)
        {
            throw new UnauthorizedAccessException();
        }

        if (currentUser.Role is UserRole.SysAdmin)
        {
            return;
        }

        var pet = await dbContext.Set<Pet>()
            .Include(p => p.Owner)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PetId == petId, cancellationToken);

        if (pet is null)
        {
            throw new KeyNotFoundException("Pet not found.");
        }

        if (currentUser.Role == UserRole.Physio)
        {
            var physioUser = await dbContext.Set<User>()
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == currentUser.UserId, cancellationToken);

            if (physioUser?.ClinicId != null && pet.Owner?.ClinicId == physioUser.ClinicId)
            {
                return;
            }

            throw new UnauthorizedAccessException("You do not have permission to access pets outside your practice.");
        }

        if (currentUser.Role == UserRole.Owner && pet.OwnerId != currentUser.UserId)
        {
            throw new UnauthorizedAccessException("You can only access your own pets.");
        }
    }
}
