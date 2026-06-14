using MenuMate.Common.Application;
using MenuMate.Contracts.RecipeImports;

namespace MenuMate.Modules.RecipeImports.Application.GetRecipeImportDraft;

/// <summary>
/// Запрос черновика импорта.
/// </summary>
public sealed record GetRecipeImportDraftQuery(Guid DraftId) : IQuery<RecipeImportDraftResponse>;
