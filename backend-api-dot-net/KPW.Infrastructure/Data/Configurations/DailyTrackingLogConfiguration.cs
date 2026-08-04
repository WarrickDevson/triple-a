using KPW.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KPW.Infrastructure.Data.Configurations;

public class DailyTrackingLogConfiguration : IEntityTypeConfiguration<DailyTrackingLog>
{
    public void Configure(EntityTypeBuilder<DailyTrackingLog> builder)
    {
        builder.ToTable("DailyTrackingLogs");
        builder.HasKey(l => l.LogId);
        builder.Property(l => l.WeightKg).HasPrecision(5, 2);

        builder.HasOne(l => l.Pet)
            .WithMany(p => p.DailyTrackingLogs)
            .HasForeignKey(l => l.PetId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
