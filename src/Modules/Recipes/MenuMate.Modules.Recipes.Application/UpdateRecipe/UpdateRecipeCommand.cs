using MenuMate.Common.Application;
using MenuMate.Contracts.Recipes;

namespace MenuMate.Modules.Recipes.Application.UpdateRecipe;

/// <summary>
/// Команда обновления рецепта.
/// </summary>
/// <param name="RecipeId">Идентификатор рецепта.</param>
/// <param name="Request">Новые данные рецепта.</param>
public sealed record UpdateRecipeCommand(Guid RecipeId, UpdateRecipeRequest Request) : ICommand;

