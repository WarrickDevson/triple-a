using KPW.Application.DTOs.Pets;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using KPW.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.Pets.Commands;

public record CreatePetCommand(CreatePetRequestDto Request) : IRequest<PetDto>;

public class CreatePetCommandHandler : IRequestHandler<CreatePetCommand, PetDto>
{
    private readonly DbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPasswordHasher _passwordHasher;

    public CreatePetCommandHandler(
        DbContext dbContext,
        ICurrentUserService currentUserService,
        IPasswordHasher passwordHasher)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _passwordHasher = passwordHasher;
    }

    public async Task<PetDto> Handle(CreatePetCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var ownerId = await ResolveOwnerId(request, cancellationToken);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            if (_currentUserService.Role is UserRole.Physio or UserRole.SysAdmin && _currentUserService.UserId.HasValue)
            {
                var physioUser = await _dbContext.Set<User>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.UserId == _currentUserService.UserId.Value, cancellationToken);
                var ownerUser = await _dbContext.Set<User>()
                    .FirstOrDefaultAsync(u => u.UserId == ownerId, cancellationToken);

                if (physioUser?.ClinicId is not null && ownerUser is not null && ownerUser.ClinicId != physioUser.ClinicId)
                {
                    ownerUser.ClinicId = physioUser.ClinicId;
                }
            }

            var pet = new Pet
            {
                OwnerId = ownerId,
                PetName = request.PetName.Trim(),
                Species = request.Species,
                Breed = request.Breed?.Trim(),
                BirthDate = request.BirthDate,
                WeightKg = request.WeightKg
            };

            _dbContext.Set<Pet>().Add(pet);

            if (request.InitialMedicalHistory is not null)
            {
                var history = new MedicalHistory
                {
                    Pet = pet,
                    Diagnosis = request.InitialMedicalHistory.Diagnosis.Trim(),
                    InjuryOrCondition = request.InitialMedicalHistory.InjuryOrCondition?.Trim(),
                    SurgeryDate = request.InitialMedicalHistory.SurgeryDate,
                    ClinicianNotes = request.InitialMedicalHistory.ClinicianNotes?.Trim()
                };
                _dbContext.Set<MedicalHistory>().Add(history);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            var created = await _dbContext.Set<Pet>()
                .Include(p => p.Owner)
                .Include(p => p.MedicalHistories)
                .FirstAsync(p => p.PetId == pet.PetId, cancellationToken);

            return PetMapper.ToDto(created);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private async Task<int> ResolveOwnerId(CreatePetRequestDto request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("Authentication is required to create a pet profile.");
        }

        if (_currentUserService.Role == UserRole.Owner)
        {
            if (request.OwnerId.HasValue && request.OwnerId != _currentUserService.UserId)
            {
                throw new UnauthorizedAccessException("Owners can only create pets for themselves.");
            }

            if (request.NewOwner is not null)
            {
                throw new InvalidOperationException("Owners cannot create new owner accounts.");
            }

            return _currentUserService.UserId!.Value;
        }

        if (request.NewOwner is not null)
        {
            if (_currentUserService.Role is not (UserRole.Physio or UserRole.SysAdmin))
            {
                throw new UnauthorizedAccessException();
            }

            var users = _dbContext.Set<User>();
            var email = request.NewOwner.Email.Trim().ToLowerInvariant();
            if (await users.AnyAsync(u => u.Email == email, cancellationToken))
            {
                throw new InvalidOperationException("An account with this email already exists.");
            }

            var currentUser = await users.FirstAsync(u => u.UserId == _currentUserService.UserId, cancellationToken);
            var owner = new User
            {
                Email = email,
                PasswordHash = _passwordHasher.HashPassword(request.NewOwner.TemporaryPassword),
                FirstName = request.NewOwner.FirstName.Trim(),
                LastName = request.NewOwner.LastName.Trim(),
                PhoneNumber = request.NewOwner.PhoneNumber?.Trim(),
                UserRole = UserRole.Owner,
                SubscriptionTier = SubscriptionTier.Free,
                ClinicId = currentUser.ClinicId,
                IsEmailVerified = true,
                IsApproved = true
            };
            users.Add(owner);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return owner.UserId;
        }

        return request.OwnerId ?? _currentUserService.UserId!.Value;
    }
}
