using MenuMate.Common.Application;
using MenuMate.Contracts.RecipeImports;

namespace MenuMate.Modules.RecipeImports.Application.GetRecipeImportDrafts;

/// <summary>
/// Запрос последних черновиков импорта текущего пользователя.
/// </summary>
public sealed record GetRecipeImportDraftsQuery : IQuery<IReadOnlyCollection<RecipeImportDraftListItemResponse>>;
