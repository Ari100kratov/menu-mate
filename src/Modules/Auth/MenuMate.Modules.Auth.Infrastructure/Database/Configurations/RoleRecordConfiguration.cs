using MenuMate.Modules.Auth.Domain.ValueObjects;
using MenuMate.Modules.Auth.Infrastructure.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MenuMate.Modules.Auth.Infrastructure.Database.Configurations;

internal sealed class RoleRecordConfiguration : IEntityTypeConfiguration<RoleRecord>
{
    public void Configure(EntityTypeBuilder<RoleRecord> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("roles");
        builder.HasKey(role => role.Id);
        builder.Property(role => role.Id).ValueGeneratedNever();
        builder.Property(role => role.Name).HasMaxLength(64).IsRequired();
        builder.HasIndex(role => role.Name).IsUnique();
        builder.HasData(
            new RoleRecord
            {
                Id = new Guid("018f0000-0000-7000-8000-000000000001"),
                Name = AuthRoleNames.User
            },
            new RoleRecord
            {
                Id = new Guid("018f0000-0000-7000-8000-000000000002"),
                Name = AuthRoleNames.Admin
            });
    }
}
