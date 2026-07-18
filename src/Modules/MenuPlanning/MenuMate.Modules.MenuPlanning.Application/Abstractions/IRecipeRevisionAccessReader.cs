using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.MenuPlanning.Application.Abstractions;

internal interface IRecipeRevisionAccessReader
{
    Task<RecipeRevisionMenuSnapshot?> GetAccessibleAsync(
        UserId userId,
        RecipeId recipeId,
        RecipeRevisionId recipeRevisionId,
        CancellationToken cancellationToken);
}

internal sealed record RecipeRevisionMenuSnapshot(string Title);
