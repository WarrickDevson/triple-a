using KPW.Application.DTOs.Appointments;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using KPW.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.Appointments.Commands;

public record UpdateAppointmentStatusCommand(int AppointmentId, UpdateAppointmentStatusRequestDto Request)
    : IRequest<AppointmentDto>;

public class UpdateAppointmentStatusCommandHandler
    : IRequestHandler<UpdateAppointmentStatusCommand, AppointmentDto>
{
    private readonly DbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public UpdateAppointmentStatusCommandHandler(
        DbContext dbContext,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<AppointmentDto> Handle(
        UpdateAppointmentStatusCommand command,
        CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is null)
        {
            throw new UnauthorizedAccessException();
        }

        var appointment = await _dbContext.Set<Appointment>()
            .Include(a => a.Physio)
            .Include(a => a.Owner)
            .Include(a => a.Pet)
            .FirstOrDefaultAsync(a => a.AppointmentId == command.AppointmentId, cancellationToken);

        if (appointment is null)
        {
            throw new KeyNotFoundException("Appointment not found.");
        }

        var newStatus = command.Request.Status;
        var role = _currentUserService.Role;

        if (role == UserRole.Owner)
        {
            if (appointment.OwnerId != _currentUserService.UserId)
            {
                throw new UnauthorizedAccessException("You can only update your own appointments.");
            }

            if (newStatus != AppointmentStatus.Cancelled)
            {
                throw new UnauthorizedAccessException("Owners can only cancel appointments.");
            }

            if (appointment.AppointmentStatus != AppointmentStatus.Scheduled)
            {
                throw new InvalidOperationException("Only scheduled appointments can be cancelled.");
            }
        }
        else if (role is UserRole.Physio or UserRole.SysAdmin)
        {
            if (role == UserRole.Physio && appointment.PhysioId != _currentUserService.UserId)
            {
                throw new UnauthorizedAccessException("You can only update your own appointments.");
            }

            if (newStatus is not (AppointmentStatus.Completed or AppointmentStatus.Cancelled))
            {
                throw new InvalidOperationException("Physiotherapists can only mark appointments as Completed or Cancelled.");
            }

            if (appointment.AppointmentStatus != AppointmentStatus.Scheduled)
            {
                throw new InvalidOperationException("Only scheduled appointments can be updated.");
            }

            if (!string.IsNullOrWhiteSpace(command.Request.ClinicianNotes))
            {
                appointment.ClinicianNotes = command.Request.ClinicianNotes.Trim();
            }
        }
        else
        {
            throw new UnauthorizedAccessException();
        }

        appointment.AppointmentStatus = newStatus;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return AppointmentMapper.ToDto(appointment);
    }
}
