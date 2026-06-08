using MenuMate.Common.Application;
using MenuMate.Contracts.Recipes;

namespace MenuMate.Modules.Recipes.Application.CopyRecipe;

/// <summary>
/// Creates a new private owned recipe from the accessible current revision.
/// </summary>
/// <param name="RecipeId">Source recipe identifier.</param>
public sealed record CopyRecipeCommand(Guid RecipeId) : ICommand<RecipeResponse>;
