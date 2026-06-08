using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.MenuPlanning.Application.Abstractions;

internal interface IRecipeRevisionAccessReader
{
    Task<bool> CanUseAsync(
        UserId userId,
        RecipeId recipeId,
        RecipeRevisionId recipeRevisionId,
        CancellationToken cancellationToken);
}
