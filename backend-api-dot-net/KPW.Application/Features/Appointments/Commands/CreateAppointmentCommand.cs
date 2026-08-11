using KPW.Application.DTOs.Appointments;
using KPW.Application.Features.Pets;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using KPW.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.Appointments.Commands;

public record CreateAppointmentCommand(CreateAppointmentRequestDto Request) : IRequest<AppointmentDto>;

public class CreateAppointmentCommandHandler : IRequestHandler<CreateAppointmentCommand, AppointmentDto>
{
    private readonly DbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public CreateAppointmentCommandHandler(DbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<AppointmentDto> Handle(CreateAppointmentCommand command, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is null)
        {
            throw new UnauthorizedAccessException();
        }

        var request = command.Request;
        await PetAuthorization.EnsureCanAccessPet(
            _dbContext, _currentUserService, request.PetId, cancellationToken);

        var pet = await _dbContext.Set<Pet>()
            .AsNoTracking()
            .FirstAsync(p => p.PetId == request.PetId, cancellationToken);

        int physioId;
        int ownerId;

        if (_currentUserService.Role is UserRole.Physio or UserRole.SysAdmin)
        {
            physioId = _currentUserService.UserId.Value;
            ownerId = pet.OwnerId;
        }
        else if (_currentUserService.Role == UserRole.Owner)
        {
            ownerId = _currentUserService.UserId.Value;
            if (pet.OwnerId != ownerId)
            {
                throw new UnauthorizedAccessException("You can only book appointments for your own pets.");
            }

            var activeProgram = await _dbContext.Set<RehabProgram>()
                .Where(p => p.PetId == request.PetId && p.IsActive)
                .OrderByDescending(p => p.StartDate)
                .FirstOrDefaultAsync(cancellationToken);

            if (activeProgram is not null)
            {
                physioId = activeProgram.PhysioId;
            }
            else
            {
                var petWithOwner = await _dbContext.Set<Pet>()
                    .Include(p => p.Owner)
                    .FirstOrDefaultAsync(p => p.PetId == request.PetId, cancellationToken);

                var clinicId = petWithOwner?.Owner?.ClinicId;
                if (clinicId is not null)
                {
                    var clinicPhysio = await _dbContext.Set<User>()
                        .Where(u => u.ClinicId == clinicId && u.UserRole == UserRole.Physio && u.IsActive)
                        .Select(u => (int?)u.UserId)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (clinicPhysio.HasValue)
                    {
                        physioId = clinicPhysio.Value;
                    }
                    else
                    {
                        throw new InvalidOperationException("No physiotherapist found in your clinic for appointment booking.");
                    }
                }
                else
                {
                    var fallbackPhysio = await _dbContext.Set<User>()
                        .Where(u => u.UserRole == UserRole.Physio && u.IsActive)
                        .Select(u => (int?)u.UserId)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (fallbackPhysio.HasValue)
                    {
                        physioId = fallbackPhysio.Value;
                    }
                    else
                    {
                        throw new InvalidOperationException("No physiotherapist is currently available for appointment scheduling.");
                    }
                }
            }
        }
        else
        {
            throw new UnauthorizedAccessException();
        }

        var appointment = new Appointment
        {
            PhysioId = physioId,
            OwnerId = ownerId,
            PetId = request.PetId,
            ScheduledDateTime = DateTime.SpecifyKind(request.ScheduledDateTime, DateTimeKind.Utc),
            AppointmentStatus = _currentUserService.Role == UserRole.Owner
                ? AppointmentStatus.Requested
                : AppointmentStatus.Scheduled,
            ClientNotes = request.ClientNotes?.Trim(),
            ClinicianNotes = _currentUserService.Role is UserRole.Physio or UserRole.SysAdmin
                ? request.ClinicianNotes?.Trim()
                : null
        };

        _dbContext.Set<Appointment>().Add(appointment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var created = await _dbContext.Set<Appointment>()
            .Include(a => a.Physio)
            .Include(a => a.Owner)
            .Include(a => a.Pet)
            .FirstAsync(a => a.AppointmentId == appointment.AppointmentId, cancellationToken);

        return AppointmentMapper.ToDto(created);
    }
}
