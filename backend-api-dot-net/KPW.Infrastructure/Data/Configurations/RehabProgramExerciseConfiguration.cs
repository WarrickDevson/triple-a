using KPW.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KPW.Infrastructure.Data.Configurations;

public class RehabProgramExerciseConfiguration : IEntityTypeConfiguration<RehabProgramExercise>
{
    public void Configure(EntityTypeBuilder<RehabProgramExercise> builder)
    {
        builder.ToTable("RehabProgramExercises");
        builder.HasKey(r => r.RehabProgramExerciseId);

        builder.HasOne(r => r.RehabProgram)
            .WithMany(p => p.RehabProgramExercises)
            .HasForeignKey(r => r.RehabProgramId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Exercise)
            .WithMany(e => e.RehabProgramExercises)
            .HasForeignKey(r => r.ExerciseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
