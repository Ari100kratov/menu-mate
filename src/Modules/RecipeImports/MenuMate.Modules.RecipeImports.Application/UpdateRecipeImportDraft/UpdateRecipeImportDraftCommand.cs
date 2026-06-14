using MenuMate.Common.Application;
using MenuMate.Contracts.RecipeImports;

namespace MenuMate.Modules.RecipeImports.Application.UpdateRecipeImportDraft;

/// <summary>
/// Команда автосохранения редактируемого черновика.
/// </summary>
public sealed record UpdateRecipeImportDraftCommand(
    Guid DraftId,
    UpdateRecipeImportDraftRequest Request) : ICommand<RecipeImportDraftResponse>;
