using KPW.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KPW.Infrastructure.Data.Configurations;

public class PetConfiguration : IEntityTypeConfiguration<Pet>
{
    public void Configure(EntityTypeBuilder<Pet> builder)
    {
        builder.ToTable("Pets");
        builder.HasKey(p => p.PetId);
        builder.Property(p => p.PetName).HasMaxLength(100).IsRequired();
        builder.Property(p => p.Species).HasMaxLength(50).IsRequired();
        builder.Property(p => p.Breed).HasMaxLength(100);
        builder.Property(p => p.WeightKg).HasPrecision(5, 2);

        builder.HasOne(p => p.Owner)
            .WithMany(u => u.Pets)
            .HasForeignKey(p => p.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
