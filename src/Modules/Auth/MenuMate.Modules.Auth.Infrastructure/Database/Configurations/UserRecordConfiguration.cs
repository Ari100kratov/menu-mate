using MenuMate.Modules.Auth.Infrastructure.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MenuMate.Modules.Auth.Infrastructure.Database.Configurations;

internal sealed class UserRecordConfiguration : IEntityTypeConfiguration<UserRecord>
{
    public void Configure(EntityTypeBuilder<UserRecord> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("users");
        builder.HasKey(user => user.Id);
        builder.Property(user => user.Id).ValueGeneratedNever();
        builder.Property(user => user.Email).HasMaxLength(320).IsRequired();
        builder.Property(user => user.DisplayName).HasMaxLength(120).IsRequired();
        builder.Property(user => user.PasswordHash).HasMaxLength(512).IsRequired();
        builder.HasIndex(user => user.Email).IsUnique();

        builder.HasMany(user => user.Roles)
            .WithOne()
            .HasForeignKey(role => role.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(user => user.RefreshTokens)
            .WithOne()
            .HasForeignKey(token => token.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
