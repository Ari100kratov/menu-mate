using MenuMate.Common.Application;
using MenuMate.Contracts.Recipes;

namespace MenuMate.Modules.Recipes.Application.GetRecipes;

/// <summary>
/// Запрос списка рецептов.
/// </summary>
/// <param name="Scope">Область поиска рецептов.</param>
/// <param name="Search">Строка поиска.</param>
/// <param name="Tag">Фильтр по тегу.</param>
/// <param name="FavoritesOnly">Возвращать только избранные рецепты.</param>
public sealed record GetRecipesQuery(
    string Scope,
    string? Search,
    string? Tag,
    bool FavoritesOnly) : IQuery<IReadOnlyCollection<RecipeListItemResponse>>;
