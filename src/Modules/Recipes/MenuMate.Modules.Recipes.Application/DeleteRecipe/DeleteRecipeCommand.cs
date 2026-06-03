using MenuMate.Common.Application;

namespace MenuMate.Modules.Recipes.Application.DeleteRecipe;

/// <summary>
/// Команда мягкого удаления рецепта.
/// </summary>
/// <param name="RecipeId">Идентификатор рецепта.</param>
public sealed record DeleteRecipeCommand(Guid RecipeId) : ICommand;

