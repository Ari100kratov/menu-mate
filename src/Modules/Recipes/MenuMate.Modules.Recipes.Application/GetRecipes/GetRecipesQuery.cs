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
/// <param name="AvailableOnly">Исключить скрытые и удаленные источники.</param>
/// <param name="Sort">Порядок сортировки списка.</param>
/// <param name="Ownership">Ограничение списка по принадлежности рецепта.</param>
/// <param name="Page">Номер страницы, начиная с единицы.</param>
/// <param name="PageSize">Количество рецептов на странице.</param>
public sealed record GetRecipesQuery(
    string Scope,
    string? Search,
    IReadOnlyCollection<Guid> TagIds,
    string? Category,
    bool FavoritesOnly,
    bool AvailableOnly,
    RecipeListSort Sort,
    RecipeOwnershipFilter Ownership,
    int Page,
    int PageSize) : IQuery<RecipeListPageResponse>;

/// <summary>
/// Поддерживаемые порядки сортировки списка рецептов.
/// </summary>
public enum RecipeListSort
{
    /// <summary>По названию по возрастанию.</summary>
    Alphabetical,
    /// <summary>Сначала недавно созданные рецепты.</summary>
    Newest,
    /// <summary>Сначала рецепты с наибольшим количеством избранных.</summary>
    Popular
}

/// <summary>
/// Ограничение списка рецептов по их владельцу относительно текущего пользователя.
/// </summary>
public enum RecipeOwnershipFilter
{
    /// <summary>Все доступные рецепты.</summary>
    All,
    /// <summary>Только рецепты текущего пользователя.</summary>
    Mine,
    /// <summary>Только рецепты других пользователей.</summary>
    Others
}
