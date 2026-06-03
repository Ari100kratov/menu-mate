using MenuMate.Modules.MenuPlanning.Infrastructure.Database.Entities;
using MenuMate.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MenuMate.Modules.MenuPlanning.Infrastructure.Database.Configurations;

internal sealed class MenuPlanRecordConfiguration : IEntityTypeConfiguration<MenuPlanRecord>
{
    public void Configure(EntityTypeBuilder<MenuPlanRecord> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("menu_plans");
        builder.HasKey(menuPlan => menuPlan.Id);
        builder.Property(menuPlan => menuPlan.Id).ValueGeneratedNever();
        builder.Property(menuPlan => menuPlan.OwnerUserId)
            .HasConversion(userId => userId.Value, value => UserId.From(value))
            .IsRequired();
        builder.Property(menuPlan => menuPlan.Name).HasMaxLength(160).IsRequired();
        builder.HasIndex(menuPlan => menuPlan.StartDate);
        builder.HasIndex(menuPlan => menuPlan.EndDate);
        builder.HasIndex(menuPlan => menuPlan.OwnerUserId);

        builder.HasMany(menuPlan => menuPlan.Items)
            .WithOne()
            .HasForeignKey(item => item.MenuPlanId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
