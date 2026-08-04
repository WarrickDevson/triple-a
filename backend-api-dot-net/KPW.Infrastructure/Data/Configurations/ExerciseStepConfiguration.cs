using KPW.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KPW.Infrastructure.Data.Configurations;

public class ExerciseStepConfiguration : IEntityTypeConfiguration<ExerciseStep>
{
    public void Configure(EntityTypeBuilder<ExerciseStep> builder)
    {
        builder.ToTable("ExerciseSteps");
        builder.HasKey(s => s.ExerciseStepId);
        builder.Property(s => s.StepInstruction).HasMaxLength(1000).IsRequired();
        builder.Property(s => s.ImageUrl).HasMaxLength(500);

        builder.HasOne(s => s.Exercise)
            .WithMany(e => e.Steps)
            .HasForeignKey(s => s.ExerciseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
