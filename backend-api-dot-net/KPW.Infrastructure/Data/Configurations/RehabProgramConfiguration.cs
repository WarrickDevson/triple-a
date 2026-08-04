using KPW.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KPW.Infrastructure.Data.Configurations;

public class RehabProgramConfiguration : IEntityTypeConfiguration<RehabProgram>
{
    public void Configure(EntityTypeBuilder<RehabProgram> builder)
    {
        builder.ToTable("RehabPrograms");
        builder.HasKey(r => r.RehabProgramId);
        builder.Property(r => r.ProgramTitle).HasMaxLength(150).IsRequired();

        builder.HasOne(r => r.Physio)
            .WithMany(u => u.RehabProgramsAsPhysio)
            .HasForeignKey(r => r.PhysioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Pet)
            .WithMany(p => p.RehabPrograms)
            .HasForeignKey(r => r.PetId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
