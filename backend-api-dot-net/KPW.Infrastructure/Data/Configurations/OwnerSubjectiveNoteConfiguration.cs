using KPW.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KPW.Infrastructure.Data.Configurations;

public class OwnerSubjectiveNoteConfiguration : IEntityTypeConfiguration<OwnerSubjectiveNote>
{
    public void Configure(EntityTypeBuilder<OwnerSubjectiveNote> builder)
    {
        builder.ToTable("OwnerSubjectiveNotes");
        builder.HasKey(n => n.OwnerSubjectiveNoteId);

        builder.Property(n => n.Notes).HasMaxLength(2000).IsRequired();

        builder.HasOne(n => n.Pet)
            .WithMany(p => p.OwnerSubjectiveNotes)
            .HasForeignKey(n => n.PetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(n => n.Owner)
            .WithMany()
            .HasForeignKey(n => n.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
