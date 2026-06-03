using MenuMate.Common.Application;
using MenuMate.Contracts.Recipes;

namespace MenuMate.Modules.Recipes.Application.GetRecipeById;

/// <summary>
/// Запрос рецепта по идентификатору.
/// </summary>
/// <param name="RecipeId">Идентификатор рецепта.</param>
public sealed record GetRecipeByIdQuery(Guid RecipeId) : IQuery<RecipeResponse>;

