using MenuMate.Common.Application;
using MenuMate.Contracts.Recipes;

namespace MenuMate.Modules.Recipes.Application.CopyRecipe;

/// <summary>
/// Creates a new owned recipe from an accessible exact revision when the edited draft is saved.
/// </summary>
/// <param name="RecipeId">Source recipe identifier.</param>
/// <param name="Request">Edited copy content and exact source revision.</param>
public sealed record CopyRecipeCommand(Guid RecipeId, CopyRecipeRequest Request) : ICommand<RecipeResponse>;
