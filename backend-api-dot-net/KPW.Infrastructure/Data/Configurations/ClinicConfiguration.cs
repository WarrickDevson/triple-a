using KPW.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KPW.Infrastructure.Data.Configurations;

public class ClinicConfiguration : IEntityTypeConfiguration<Clinic>
{
    public void Configure(EntityTypeBuilder<Clinic> builder)
    {
        builder.ToTable("Clinics");
        builder.HasKey(c => c.ClinicId);
        builder.Property(c => c.ClinicName).HasMaxLength(150).IsRequired();
        builder.Property(c => c.VatNumber).HasMaxLength(50);
        builder.Property(c => c.PhysicalAddress).HasMaxLength(500).IsRequired();
        builder.Property(c => c.ContactNumber).HasMaxLength(20).IsRequired();
        builder.Property(c => c.InviteCode).HasMaxLength(16).IsRequired();
        builder.HasIndex(c => c.InviteCode).IsUnique();
    }
}
