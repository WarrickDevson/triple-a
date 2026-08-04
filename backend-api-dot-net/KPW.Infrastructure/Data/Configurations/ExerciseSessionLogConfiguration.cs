using KPW.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KPW.Infrastructure.Data.Configurations;

public class ExerciseSessionLogConfiguration : IEntityTypeConfiguration<ExerciseSessionLog>
{
    public void Configure(EntityTypeBuilder<ExerciseSessionLog> builder)
    {
        builder.ToTable("ExerciseSessionLogs");
        builder.HasKey(l => l.ExerciseSessionLogId);
        builder.HasIndex(l => new { l.PetId, l.ExerciseId, l.CompletedAt });

        builder.HasOne(l => l.Pet)
            .WithMany(p => p.ExerciseSessionLogs)
            .HasForeignKey(l => l.PetId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.Exercise)
            .WithMany()
            .HasForeignKey(l => l.ExerciseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.RehabProgram)
            .WithMany()
            .HasForeignKey(l => l.RehabProgramId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
