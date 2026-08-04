using KPW.Application;
using KPW.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KPW.Infrastructure.Data;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Clinic> Clinics => Set<Clinic>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Pet> Pets => Set<Pet>();
    public DbSet<MedicalHistory> MedicalHistories => Set<MedicalHistory>();
    public DbSet<Exercise> Exercises => Set<Exercise>();
    public DbSet<ExerciseStep> ExerciseSteps => Set<ExerciseStep>();
    public DbSet<RehabProgram> RehabPrograms => Set<RehabProgram>();
    public DbSet<RehabProgramExercise> RehabProgramExercises => Set<RehabProgramExercise>();
    public DbSet<DailyTrackingLog> DailyTrackingLogs => Set<DailyTrackingLog>();
    public DbSet<VideoSubmission> VideoSubmissions => Set<VideoSubmission>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<MessageThread> MessageThreads => Set<MessageThread>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<ExerciseSessionLog> ExerciseSessionLogs => Set<ExerciseSessionLog>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        ApplySoftDeleteFilters(modelBuilder);
        DatabaseSeeder.Seed(modelBuilder);
    }

    private static void ApplySoftDeleteFilters(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Clinic>().HasQueryFilter(e => e.IsActive);
        modelBuilder.Entity<User>().HasQueryFilter(e => e.IsActive);
        modelBuilder.Entity<Pet>().HasQueryFilter(e => e.IsActive);
        modelBuilder.Entity<MedicalHistory>().HasQueryFilter(e => e.IsActive);
        modelBuilder.Entity<Exercise>().HasQueryFilter(e => e.IsActive);
        modelBuilder.Entity<ExerciseStep>().HasQueryFilter(e => e.IsActive);
        modelBuilder.Entity<RehabProgram>().HasQueryFilter(e => e.IsActive);
        modelBuilder.Entity<RehabProgramExercise>().HasQueryFilter(e => e.IsActive);
        modelBuilder.Entity<DailyTrackingLog>().HasQueryFilter(e => e.IsActive);
        modelBuilder.Entity<VideoSubmission>().HasQueryFilter(e => e.IsActive);
        modelBuilder.Entity<Appointment>().HasQueryFilter(e => e.IsActive);
        modelBuilder.Entity<MessageThread>().HasQueryFilter(e => e.IsActive);
        modelBuilder.Entity<Message>().HasQueryFilter(e => e.IsActive);
        modelBuilder.Entity<ExerciseSessionLog>().HasQueryFilter(e => e.IsActive);
    }
}
