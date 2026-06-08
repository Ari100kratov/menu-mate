using MenuMate.Modules.Recipes.Infrastructure.Database.Entities;
using MenuMate.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MenuMate.Modules.Recipes.Infrastructure.Database.Configurations;

internal sealed class RecipeLibraryEntryRecordConfiguration : IEntityTypeConfiguration<RecipeLibraryEntryRecord>
{
    public void Configure(EntityTypeBuilder<RecipeLibraryEntryRecord> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("recipe_library_entries");
        builder.HasKey(entry => new { entry.UserId, entry.RecipeId });
        builder.Property(entry => entry.UserId)
            .HasConversion(userId => userId.Value, value => UserId.From(value))
            .IsRequired();
        builder.HasIndex(entry => new { entry.UserId, entry.IsFavorite });
        builder.HasOne<RecipeRecord>()
            .WithMany(recipe => recipe.LibraryEntries)
            .HasForeignKey(entry => entry.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
