using MenuMate.Common.Application;

namespace MenuMate.Modules.RecipeImports.Application.DeleteRecipeImportDraft;

/// <summary>
/// Команда удаления черновика импорта.
/// </summary>
public sealed record DeleteRecipeImportDraftCommand(Guid DraftId) : ICommand;
