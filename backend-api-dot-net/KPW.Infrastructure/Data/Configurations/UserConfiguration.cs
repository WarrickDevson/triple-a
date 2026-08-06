using KPW.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KPW.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.UserId);
        builder.HasIndex(u => u.Email).IsUnique();
        builder.Property(u => u.Email).HasMaxLength(256).IsRequired();
        builder.Property(u => u.PasswordHash).HasMaxLength(500).IsRequired();
        builder.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(u => u.LastName).HasMaxLength(100).IsRequired();
        builder.Property(u => u.PhoneNumber).HasMaxLength(20);
        builder.Property(u => u.UserRole).HasMaxLength(50).IsRequired();
        builder.Property(u => u.SubscriptionTier).HasMaxLength(50).IsRequired();
        builder.Property(u => u.RefreshTokenHash).HasMaxLength(500);
        builder.Property(u => u.IsEmailVerified).HasDefaultValue(false);
        builder.Property(u => u.EmailVerificationTokenHash).HasMaxLength(500);

        builder.HasOne(u => u.Clinic)
            .WithMany(c => c.Users)
            .HasForeignKey(u => u.ClinicId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
