using MenuMate.Common.Application;
using MenuMate.Contracts.Recipes;

namespace MenuMate.Modules.Recipes.Application.GetRecipes;

/// <summary>
/// Запрос списка рецептов.
/// </summary>
/// <param name="Scope">Область поиска рецептов.</param>
/// <param name="Search">Строка поиска.</param>
/// <param name="TagIds">Фильтр по идентификаторам глобальных тегов с логикой «или».</param>
/// <param name="Category">Фильтр по категории рецепта.</param>
/// <param name="FavoritesOnly">Возвращать только избранные рецепты.</param>
/// <param name="Page">Номер страницы, начиная с единицы.</param>
/// <param name="PageSize">Количество рецептов на странице.</param>
public sealed record GetRecipesQuery(
    string Scope,
    string? Search,
    IReadOnlyCollection<Guid> TagIds,
    string? Category,
    bool FavoritesOnly,
    int Page,
    int PageSize) : IQuery<IReadOnlyCollection<RecipeListItemResponse>>;
