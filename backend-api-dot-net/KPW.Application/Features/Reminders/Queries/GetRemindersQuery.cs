using KPW.Application.DTOs.Reminders;
using KPW.Application.Interfaces;
using KPW.Domain.Entities;
using KPW.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPW.Application.Features.Reminders.Queries;

public record GetRemindersQuery : IRequest<IReadOnlyList<ReminderDto>>;

public class GetRemindersQueryHandler : IRequestHandler<GetRemindersQuery, IReadOnlyList<ReminderDto>>
{
    private readonly DbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetRemindersQueryHandler(DbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<ReminderDto>> Handle(
        GetRemindersQuery request,
        CancellationToken cancellationToken)
    {
        if (_currentUserService.Role != UserRole.Owner)
        {
            throw new UnauthorizedAccessException("Only pet owners can view reminders.");
        }

        var ownerId = _currentUserService.UserId!.Value;
        var reminders = new List<ReminderDto>();
        var now = DateTime.UtcNow;
        var windowEnd = now.AddHours(24);
        var todayStart = now.Date;
        var todayEnd = todayStart.AddDays(1);

        var pets = await _dbContext.Set<Pet>()
            .Where(p => p.OwnerId == ownerId)
            .ToListAsync(cancellationToken);

        var petIds = pets.Select(p => p.PetId).ToList();

        var appointments = await _dbContext.Set<Appointment>()
            .Include(a => a.Pet)
            .Where(a =>
                petIds.Contains(a.PetId) &&
                a.AppointmentStatus == AppointmentStatus.Scheduled &&
                a.ScheduledDateTime >= now &&
                a.ScheduledDateTime <= windowEnd)
            .OrderBy(a => a.ScheduledDateTime)
            .ToListAsync(cancellationToken);

        foreach (var appointment in appointments)
        {
            reminders.Add(new ReminderDto(
                "Appointment",
                $"Upcoming visit for {appointment.Pet.PetName}",
                appointment.ClientNotes ?? "You have a physiotherapy appointment coming up.",
                appointment.PetId,
                appointment.Pet.PetName,
                appointment.ScheduledDateTime,
                appointment.AppointmentId));
        }

        var activePrograms = await _dbContext.Set<RehabProgram>()
            .Include(p => p.RehabProgramExercises)
                .ThenInclude(e => e.Exercise)
            .Include(p => p.Pet)
            .Where(p => petIds.Contains(p.PetId) && p.IsActive)
            .ToListAsync(cancellationToken);

        var completionCounts = await _dbContext.Set<ExerciseSessionLog>()
            .Where(l =>
                petIds.Contains(l.PetId) &&
                l.CompletedAt >= todayStart &&
                l.CompletedAt < todayEnd)
            .GroupBy(l => new { l.PetId, l.ExerciseId })
            .Select(g => new { g.Key.PetId, g.Key.ExerciseId, Count = g.Count() })
            .ToListAsync(cancellationToken);

        foreach (var program in activePrograms)
        {
            foreach (var programExercise in program.RehabProgramExercises)
            {
                var completedToday = completionCounts
                    .FirstOrDefault(c =>
                        c.PetId == program.PetId &&
                        c.ExerciseId == programExercise.ExerciseId)
                    ?.Count ?? 0;

                if (completedToday >= programExercise.FrequencyPerDay)
                {
                    continue;
                }

                var remaining = programExercise.FrequencyPerDay - completedToday;
                reminders.Add(new ReminderDto(
                    "Exercise",
                    $"{programExercise.Exercise.Title} due for {program.Pet.PetName}",
                    remaining == 1
                        ? "Complete today's exercise session."
                        : $"Complete {remaining} more session(s) today.",
                    program.PetId,
                    program.Pet.PetName,
                    todayEnd,
                    programExercise.ExerciseId));
            }
        }

        return reminders
            .OrderBy(r => r.DueAt ?? DateTime.MaxValue)
            .ToList();
    }
}
