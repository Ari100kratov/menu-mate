using MenuMate.Modules.MenuPlanning.Infrastructure.Database.Source;
using MenuMate.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MenuMate.Modules.MenuPlanning.Infrastructure.Database.Configurations;

internal sealed class RecipeAccessSourceRecordConfiguration : IEntityTypeConfiguration<RecipeAccessSourceRecord>
{
    public void Configure(EntityTypeBuilder<RecipeAccessSourceRecord> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.ToTable("recipes", "recipes", table => table.ExcludeFromMigrations());
        builder.HasKey(recipe => recipe.Id);
        builder.Property(recipe => recipe.Id)
            .HasConversion(recipeId => recipeId.Value, value => RecipeId.From(value));
        builder.Property(recipe => recipe.OwnerUserId)
            .HasConversion(userId => userId.Value, value => UserId.From(value));
        builder.Property(recipe => recipe.Visibility).HasColumnName("visibility");
    }
}
