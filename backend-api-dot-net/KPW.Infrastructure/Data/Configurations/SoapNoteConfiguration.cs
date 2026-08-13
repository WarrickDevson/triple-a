using KPW.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KPW.Infrastructure.Data.Configurations;

public class SoapNoteConfiguration : IEntityTypeConfiguration<SoapNote>
{
    public void Configure(EntityTypeBuilder<SoapNote> builder)
    {
        builder.ToTable("SoapNotes");
        builder.HasKey(s => s.SoapNoteId);

        builder.Property(s => s.Subjective).HasMaxLength(4000);
        builder.Property(s => s.Objective).HasMaxLength(4000);
        builder.Property(s => s.Action).HasMaxLength(4000);
        builder.Property(s => s.Plan).HasMaxLength(4000);

        builder.HasOne(s => s.Pet)
            .WithMany(p => p.SoapNotes)
            .HasForeignKey(s => s.PetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Physio)
            .WithMany(u => u.SoapNotesAsPhysio)
            .HasForeignKey(s => s.PhysioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Appointment)
            .WithMany()
            .HasForeignKey(s => s.AppointmentId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
