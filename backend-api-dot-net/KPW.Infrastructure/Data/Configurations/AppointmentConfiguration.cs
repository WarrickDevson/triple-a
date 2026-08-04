using KPW.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KPW.Infrastructure.Data.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("Appointments");
        builder.HasKey(a => a.AppointmentId);
        builder.Property(a => a.AppointmentStatus).HasMaxLength(50).IsRequired();
        builder.Property(a => a.ClientNotes).HasMaxLength(500);

        builder.HasOne(a => a.Physio)
            .WithMany(u => u.AppointmentsAsPhysio)
            .HasForeignKey(a => a.PhysioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Owner)
            .WithMany(u => u.AppointmentsAsOwner)
            .HasForeignKey(a => a.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Pet)
            .WithMany(p => p.Appointments)
            .HasForeignKey(a => a.PetId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
