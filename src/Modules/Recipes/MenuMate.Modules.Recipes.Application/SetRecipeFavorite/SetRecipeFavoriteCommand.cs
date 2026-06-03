using MenuMate.Common.Application;

namespace MenuMate.Modules.Recipes.Application.SetRecipeFavorite;

/// <summary>
/// Команда изменения признака избранного рецепта.
/// </summary>
/// <param name="RecipeId">Идентификатор рецепта.</param>
/// <param name="IsFavorite">Новый признак избранного.</param>
public sealed record SetRecipeFavoriteCommand(Guid RecipeId, bool IsFavorite) : ICommand;

