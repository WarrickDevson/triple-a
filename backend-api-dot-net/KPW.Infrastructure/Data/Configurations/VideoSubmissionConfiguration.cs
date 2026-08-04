using KPW.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KPW.Infrastructure.Data.Configurations;

public class VideoSubmissionConfiguration : IEntityTypeConfiguration<VideoSubmission>
{
    public void Configure(EntityTypeBuilder<VideoSubmission> builder)
    {
        builder.ToTable("VideoSubmissions");
        builder.HasKey(v => v.VideoSubmissionId);
        builder.Property(v => v.RawVideoStorageUrl).HasMaxLength(500).IsRequired();
        builder.Property(v => v.ProcessedVideoStreamingUrl).HasMaxLength(500);
        builder.Property(v => v.ProcessingStatus)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(Domain.Enums.VideoProcessingStatus.Pending);

        builder.HasOne(v => v.Pet)
            .WithMany(p => p.VideoSubmissions)
            .HasForeignKey(v => v.PetId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(v => v.Exercise)
            .WithMany(e => e.VideoSubmissions)
            .HasForeignKey(v => v.ExerciseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
