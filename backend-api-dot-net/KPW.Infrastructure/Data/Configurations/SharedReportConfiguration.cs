using KPW.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KPW.Infrastructure.Data.Configurations;

public class SharedReportConfiguration : IEntityTypeConfiguration<SharedReport>
{
    public void Configure(EntityTypeBuilder<SharedReport> builder)
    {
        builder.ToTable("SharedReports");
        builder.HasKey(r => r.SharedReportId);

        builder.Property(r => r.Title).HasMaxLength(200).IsRequired();
        builder.Property(r => r.ReportType).HasMaxLength(50).IsRequired();
        builder.Property(r => r.Summary).HasMaxLength(1000);

        builder.HasOne(r => r.Pet)
            .WithMany(p => p.SharedReports)
            .HasForeignKey(r => r.PetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.SoapNote)
            .WithMany()
            .HasForeignKey(r => r.SoapNoteId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(r => r.SharedByPhysio)
            .WithMany(u => u.SharedReportsAsPhysio)
            .HasForeignKey(r => r.SharedByPhysioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
