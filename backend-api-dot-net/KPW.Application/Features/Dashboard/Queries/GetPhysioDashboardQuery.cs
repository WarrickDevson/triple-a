using KPW.Application.DTOs.Dashboard;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using KPW.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.Dashboard.Queries;

public record GetPhysioDashboardQuery : IRequest<PhysioDashboardDto>;

public class GetPhysioDashboardQueryHandler : IRequestHandler<GetPhysioDashboardQuery, PhysioDashboardDto>
{
    private readonly DbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetPhysioDashboardQueryHandler(DbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<PhysioDashboardDto> Handle(GetPhysioDashboardQuery request, CancellationToken cancellationToken)
    {
        if (_currentUserService.Role is not (UserRole.Physio or UserRole.SysAdmin))
        {
            throw new UnauthorizedAccessException();
        }

        var currentUser = await _dbContext.Set<User>()
            .AsNoTracking()
            .FirstAsync(u => u.UserId == _currentUserService.UserId, cancellationToken);

        if (currentUser.ClinicId is null)
        {
            return new PhysioDashboardDto(0, 0, 0, []);
        }

        var petsQuery = _dbContext.Set<Pet>()
            .Where(p => p.Owner.ClinicId == currentUser.ClinicId);

        var patientCount = await petsQuery.CountAsync(cancellationToken);

        var petIds = await petsQuery.Select(p => p.PetId).ToListAsync(cancellationToken);
        var pendingReviews = await _dbContext.Set<VideoSubmission>()
            .Where(v => petIds.Contains(v.PetId) && !v.IsReviewed)
            .CountAsync(cancellationToken);

        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var appointments = await _dbContext.Set<Appointment>()
            .Include(a => a.Pet)
            .Include(a => a.Owner)
            .Where(a =>
                a.PhysioId == currentUser.UserId &&
                a.ScheduledDateTime >= today &&
                a.ScheduledDateTime < tomorrow)
            .OrderBy(a => a.ScheduledDateTime)
            .Select(a => new DashboardAppointmentDto(
                a.AppointmentId,
                a.Pet.PetName,
                $"{a.Owner.FirstName} {a.Owner.LastName}",
                a.ScheduledDateTime,
                a.AppointmentStatus))
            .ToListAsync(cancellationToken);

        return new PhysioDashboardDto(
            patientCount,
            pendingReviews,
            appointments.Count,
            appointments);
    }
}
