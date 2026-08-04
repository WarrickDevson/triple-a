using KPW.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KPW.Infrastructure.Data.Configurations;

public class MessageThreadConfiguration : IEntityTypeConfiguration<MessageThread>
{
    public void Configure(EntityTypeBuilder<MessageThread> builder)
    {
        builder.ToTable("MessageThreads");
        builder.HasKey(t => t.MessageThreadId);
        builder.HasIndex(t => t.PetId).IsUnique();

        builder.HasOne(t => t.Pet)
            .WithOne(p => p.MessageThread)
            .HasForeignKey<MessageThread>(t => t.PetId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Owner)
            .WithMany()
            .HasForeignKey(t => t.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Physio)
            .WithMany()
            .HasForeignKey(t => t.PhysioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
