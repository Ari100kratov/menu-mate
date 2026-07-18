using MenuMate.Modules.Recipes.Infrastructure.Database.Source;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MenuMate.Modules.Recipes.Infrastructure.Database.Configurations;

internal sealed class RecipeMenuItemSourceRecordConfiguration
    : IEntityTypeConfiguration<RecipeMenuItemSourceRecord>
{
    public void Configure(EntityTypeBuilder<RecipeMenuItemSourceRecord> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable(
            "menu_calendar_items",
            "menu_planning",
            table => table.ExcludeFromMigrations());
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Id).ValueGeneratedNever();
    }
}
