using MenuMate.Modules.ShoppingLists.Infrastructure.Database.Source;
using MenuMate.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MenuMate.Modules.ShoppingLists.Infrastructure.Database.Configurations;

internal sealed class MenuCalendarItemSourceRecordConfiguration
    : IEntityTypeConfiguration<MenuCalendarItemSourceRecord>
{
    public void Configure(EntityTypeBuilder<MenuCalendarItemSourceRecord> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var recipeIdConverter = new ValueConverter<RecipeId?, Guid?>(
            recipeId => recipeId.HasValue ? recipeId.Value.Value : null,
            value => value.HasValue ? RecipeId.From(value.Value) : null);
        var recipeRevisionIdConverter = new ValueConverter<RecipeRevisionId?, Guid?>(
            revisionId => revisionId.HasValue ? revisionId.Value.Value : null,
            value => value.HasValue ? RecipeRevisionId.From(value.Value) : null);

        builder.ToTable("menu_calendar_items", "menu_planning", table => table.ExcludeFromMigrations());
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Id).ValueGeneratedNever();
        builder.Property(item => item.OwnerUserId)
            .HasConversion(userId => userId.Value, value => UserId.From(value));
        builder.Property(item => item.RecipeId).HasConversion(recipeIdConverter);
        builder.Property(item => item.RecipeRevisionId).HasConversion(recipeRevisionIdConverter);
        builder.Property(item => item.RecipeTitle).HasMaxLength(160);
    }
}
