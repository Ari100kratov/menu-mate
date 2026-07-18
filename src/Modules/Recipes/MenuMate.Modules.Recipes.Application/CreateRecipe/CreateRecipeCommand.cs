using MenuMate.Common.Application;
using MenuMate.Common.Application.Tags;
using MenuMate.Contracts.Recipes;

namespace MenuMate.Modules.Recipes.Application.CreateRecipe;

/// <summary>
/// Команда создания рецепта.
/// </summary>
/// <param name="Request">Данные рецепта.</param>
/// <param name="RecipeId">Необязательный заранее назначенный идентификатор рецепта.</param>
/// <param name="TagSource">Источник регистрации новых тегов в общем каталоге.</param>
public sealed record CreateRecipeCommand(
    CreateRecipeRequest Request,
    Guid? RecipeId = null,
    TagCatalogSource TagSource = TagCatalogSource.User) : ICommand<RecipeResponse>;
