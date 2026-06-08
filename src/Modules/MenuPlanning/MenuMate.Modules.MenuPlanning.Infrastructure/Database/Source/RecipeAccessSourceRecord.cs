using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.MenuPlanning.Infrastructure.Database.Source;

internal sealed class RecipeAccessSourceRecord
{
    public RecipeId Id { get; set; }
    public UserId OwnerUserId { get; set; }
    public string Visibility { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
}
