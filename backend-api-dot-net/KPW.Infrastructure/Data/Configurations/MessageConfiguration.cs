using KPW.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KPW.Infrastructure.Data.Configurations;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("Messages");
        builder.HasKey(m => m.MessageId);
        builder.Property(m => m.Body).HasMaxLength(2000).IsRequired();

        builder.HasOne(m => m.Thread)
            .WithMany(t => t.Messages)
            .HasForeignKey(m => m.MessageThreadId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Sender)
            .WithMany()
            .HasForeignKey(m => m.SenderUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.VideoSubmission)
            .WithMany()
            .HasForeignKey(m => m.VideoSubmissionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
