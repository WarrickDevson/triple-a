using KPW.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KPW.Infrastructure.Data.Configurations;

public class MedicalHistoryConfiguration : IEntityTypeConfiguration<MedicalHistory>
{
    public void Configure(EntityTypeBuilder<MedicalHistory> builder)
    {
        builder.ToTable("MedicalHistories");
        builder.HasKey(m => m.MedicalHistoryId);
        builder.Property(m => m.Diagnosis).HasMaxLength(250).IsRequired();

        builder.HasOne(m => m.Pet)
            .WithMany(p => p.MedicalHistories)
            .HasForeignKey(m => m.PetId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
