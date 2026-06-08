using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.Recipes.Infrastructure.Database.Entities;

internal sealed class RecipeLibraryEntryRecord
{
    public UserId UserId { get; set; }

    public Guid RecipeId { get; set; }

    public bool IsFavorite { get; set; }

    public DateTimeOffset SavedAt { get; set; }
}
