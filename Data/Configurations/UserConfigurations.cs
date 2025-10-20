using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Common.Constraints;
using Common.Enums;

namespace Data.Configurations;

public class UserConfigurations : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.ToTable("Users");
        
        builder.HasKey(u => u.Id);
        
        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(UserConstraints.NameMaxLength);
        
        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(UserConstraints.NameMaxLength);
        
        builder.Property(u => u.Login)
            .IsRequired()
            .HasMaxLength(UserConstraints.LoginMaxLength);
        builder.HasIndex(u => u.Login)
            .IsUnique();
        
        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(UserConstraints.PasswordHashLength);
        
        builder.Property(u => u.AccessRequestedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP AT TIME ZONE 'UTC'");

        builder.Property(u => u.Status)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(UserStatus.None);
    }
}