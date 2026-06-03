using MenuMate.Modules.Auth.Infrastructure.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MenuMate.Modules.Auth.Infrastructure.Database.Configurations;

internal sealed class RefreshTokenRecordConfiguration : IEntityTypeConfiguration<RefreshTokenRecord>
{
    public void Configure(EntityTypeBuilder<RefreshTokenRecord> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("refresh_tokens");
        builder.HasKey(token => token.Id);
        builder.Property(token => token.Id).ValueGeneratedNever();
        builder.Property(token => token.Value).HasMaxLength(512).IsRequired();
        builder.HasIndex(token => token.Value).IsUnique();
        builder.HasIndex(token => token.UserId);
    }
}
