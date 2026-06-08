using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.MenuPlanning.Infrastructure.Database.Source;

internal sealed class RecipeRevisionAccessSourceRecord
{
    public RecipeRevisionId Id { get; set; }
    public RecipeId RecipeId { get; set; }
}
