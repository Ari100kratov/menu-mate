using MenuMate.Modules.MenuPlanning.Infrastructure.Database.Entities;
using MenuMate.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MenuMate.Modules.MenuPlanning.Infrastructure.Database.Configurations;

internal sealed class MenuPlanItemRecordConfiguration : IEntityTypeConfiguration<MenuPlanItemRecord>
{
    public void Configure(EntityTypeBuilder<MenuPlanItemRecord> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var recipeIdConverter = new ValueConverter<RecipeId?, Guid?>(
            recipeId => recipeId.HasValue ? recipeId.Value.Value : null,
            value => value.HasValue ? RecipeId.From(value.Value) : null);
        var recipeRevisionIdConverter = new ValueConverter<RecipeRevisionId?, Guid?>(
            revisionId => revisionId.HasValue ? revisionId.Value.Value : null,
            value => value.HasValue ? RecipeRevisionId.From(value.Value) : null);

        builder.ToTable("menu_plan_items");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Id).ValueGeneratedNever();
        builder.Property(item => item.MealType).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(item => item.RecipeId).HasConversion(recipeIdConverter);
        builder.Property(item => item.RecipeRevisionId).HasConversion(recipeRevisionIdConverter);
        builder.Property(item => item.RecipeTitle).HasMaxLength(160);
        builder.Property(item => item.Text).HasMaxLength(500);
        builder.Property(item => item.Comment).HasMaxLength(500);
        builder.HasIndex(item => new { item.MenuPlanId, item.Date, item.MealType });
        builder.HasIndex(item => item.RecipeId);
        builder.HasIndex(item => item.RecipeRevisionId);
    }
}
