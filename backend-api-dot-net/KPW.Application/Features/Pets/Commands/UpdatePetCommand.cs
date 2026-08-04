using KPW.Application.DTOs.Pets;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.Pets.Commands;

public record UpdatePetCommand(int PetId, UpdatePetRequestDto Request) : IRequest<PetDto>;

public class UpdatePetCommandHandler : IRequestHandler<UpdatePetCommand, PetDto>
{
    private readonly DbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public UpdatePetCommandHandler(DbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<PetDto> Handle(UpdatePetCommand command, CancellationToken cancellationToken)
    {
        await PetAuthorization.EnsureCanAccessPet(
            _dbContext, _currentUserService, command.PetId, cancellationToken);

        var pet = await _dbContext.Set<Pet>()
            .Include(p => p.Owner)
            .Include(p => p.MedicalHistories)
            .FirstOrDefaultAsync(p => p.PetId == command.PetId, cancellationToken);

        if (pet is null)
        {
            throw new KeyNotFoundException("Pet not found.");
        }

        var request = command.Request;
        pet.PetName = request.PetName.Trim();
        pet.Species = request.Species;
        pet.Breed = request.Breed?.Trim();
        pet.BirthDate = request.BirthDate;
        pet.WeightKg = request.WeightKg;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return PetMapper.ToDto(pet);
    }
}
