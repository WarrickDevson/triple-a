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
    }
}
