using KPW.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KPW.Infrastructure.Data.Configurations;

public class ExerciseConfiguration : IEntityTypeConfiguration<Exercise>
{
    public void Configure(EntityTypeBuilder<Exercise> builder)
    {
        builder.ToTable("Exercises");
        builder.HasKey(e => e.ExerciseId);
        builder.Property(e => e.Title).HasMaxLength(150).IsRequired();
        builder.Property(e => e.ShortDescription).HasMaxLength(500);
        builder.Property(e => e.TargetedMuscles).HasMaxLength(250);
        builder.Property(e => e.ClinicalPurpose).HasMaxLength(500);
        builder.Property(e => e.VideoUrl).HasMaxLength(500);
        builder.Property(e => e.TargetSpecies).HasMaxLength(50);
        builder.Property(e => e.ConditionCategory).HasMaxLength(100);
        builder.Property(e => e.CoverImageUrl).HasMaxLength(500);
        builder.Property(e => e.IsSystemDefault).HasDefaultValue(true);
        builder.Property(e => e.IsActiveForOwners).HasDefaultValue(true);

        builder.HasOne(e => e.Clinic)
            .WithMany()
            .HasForeignKey(e => e.ClinicId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.BaseExercise)
            .WithMany(e => e.CustomOverrides)
            .HasForeignKey(e => e.BaseExerciseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
