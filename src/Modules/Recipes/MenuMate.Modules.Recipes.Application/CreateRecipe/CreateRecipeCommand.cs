using MenuMate.Common.Application;
using MenuMate.Contracts.Recipes;

namespace MenuMate.Modules.Recipes.Application.CreateRecipe;

/// <summary>
/// Команда создания рецепта.
/// </summary>
/// <param name="Request">Данные рецепта.</param>
public sealed record CreateRecipeCommand(CreateRecipeRequest Request) : ICommand<RecipeResponse>;

