using MenuMate.Modules.Auth.Infrastructure.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MenuMate.Modules.Auth.Infrastructure.Database.Configurations;

internal sealed class UserRoleRecordConfiguration : IEntityTypeConfiguration<UserRoleRecord>
{
    public void Configure(EntityTypeBuilder<UserRoleRecord> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("user_roles");
        builder.HasKey(role => new { role.UserId, role.RoleId });
        builder.HasOne(role => role.Role)
            .WithMany(role => role.Users)
            .HasForeignKey(role => role.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
