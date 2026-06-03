using MenuMate.Modules.Recipes.Infrastructure.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MenuMate.Modules.Recipes.Infrastructure.Database.Configurations;

internal sealed class PreparationStepRecordConfiguration : IEntityTypeConfiguration<PreparationStepRecord>
{
    public void Configure(EntityTypeBuilder<PreparationStepRecord> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("preparation_steps");
        builder.HasKey(step => step.Id);
        builder.Property(step => step.Id).ValueGeneratedNever();
        builder.Property(step => step.Text).HasMaxLength(4000).IsRequired();
        builder.HasIndex(step => new { step.RecipeId, step.Number }).IsUnique();
    }
}
