using KPW.Application.DTOs.Appointments;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using KPW.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.Appointments.Queries;

public record GetAppointmentsQuery(DateTime? From, DateTime? To) : IRequest<IReadOnlyList<AppointmentDto>>;

public class GetAppointmentsQueryHandler : IRequestHandler<GetAppointmentsQuery, IReadOnlyList<AppointmentDto>>
{
    private readonly DbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetAppointmentsQueryHandler(DbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<AppointmentDto>> Handle(
        GetAppointmentsQuery request,
        CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is null)
        {
            throw new UnauthorizedAccessException();
        }

        var from = request.From ?? DateTime.UtcNow.Date;
        var to = request.To ?? from.AddDays(30);

        var query = _dbContext.Set<Appointment>()
            .Include(a => a.Physio)
            .Include(a => a.Owner)
            .Include(a => a.Pet)
            .Where(a => a.ScheduledDateTime >= from && a.ScheduledDateTime <= to);

        if (_currentUserService.Role == UserRole.Owner)
        {
            query = query.Where(a => a.OwnerId == _currentUserService.UserId);
        }
        else if (_currentUserService.Role is UserRole.Physio or UserRole.SysAdmin)
        {
            var currentUser = await _dbContext.Set<User>()
                .AsNoTracking()
                .FirstAsync(u => u.UserId == _currentUserService.UserId, cancellationToken);

            if (currentUser.ClinicId is null)
            {
                return [];
            }

            query = query.Where(a => a.Pet.Owner.ClinicId == currentUser.ClinicId);
        }
        else
        {
            throw new UnauthorizedAccessException();
        }

        var appointments = await query
            .OrderBy(a => a.ScheduledDateTime)
            .ToListAsync(cancellationToken);

        return appointments.Select(AppointmentMapper.ToDto).ToList();
    }
}
