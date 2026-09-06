using MenuMate.Modules.Auth.Infrastructure.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MenuMate.Modules.Auth.Infrastructure.Database.Configurations;

internal sealed class AccountActionRecordConfiguration : IEntityTypeConfiguration<AccountActionRecord>
{
    public void Configure(EntityTypeBuilder<AccountActionRecord> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("account_actions");
        builder.HasKey(action => action.Id);
        builder.Property(action => action.Id).ValueGeneratedNever();
        builder.Property(action => action.Purpose).HasConversion<string>().HasMaxLength(64).IsRequired();
        builder.Property(action => action.TargetEmail).HasMaxLength(320);
        builder.Property(action => action.SecretHash).HasMaxLength(64).IsRequired();
        builder.HasIndex(action => new { action.UserId, action.Purpose, action.CreatedAt });
        builder.HasIndex(action => new { action.Purpose, action.SecretHash }).IsUnique();
        builder.HasOne<UserRecord>()
            .WithMany()
            .HasForeignKey(action => action.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
