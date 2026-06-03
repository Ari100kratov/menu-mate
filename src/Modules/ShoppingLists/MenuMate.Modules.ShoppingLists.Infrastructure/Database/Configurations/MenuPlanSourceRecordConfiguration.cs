using MenuMate.Modules.ShoppingLists.Infrastructure.Database.Source;
using MenuMate.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MenuMate.Modules.ShoppingLists.Infrastructure.Database.Configurations;

internal sealed class MenuPlanSourceRecordConfiguration : IEntityTypeConfiguration<MenuPlanSourceRecord>
{
    public void Configure(EntityTypeBuilder<MenuPlanSourceRecord> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("menu_plans", "menu_planning", table => table.ExcludeFromMigrations());
        builder.HasKey(menuPlan => menuPlan.Id);
        builder.Property(menuPlan => menuPlan.Id)
            .ValueGeneratedNever()
            .HasConversion(menuPlanId => menuPlanId.Value, value => MenuPlanId.From(value));
        builder.Property(menuPlan => menuPlan.OwnerUserId)
            .HasConversion(userId => userId.Value, value => UserId.From(value))
            .IsRequired();

        builder.HasMany(menuPlan => menuPlan.Items)
            .WithOne()
            .HasForeignKey(item => item.MenuPlanId);
    }
}
