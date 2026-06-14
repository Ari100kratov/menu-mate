using MenuMate.Common.Application;
using MenuMate.Contracts.Recipes;

namespace MenuMate.Modules.RecipeImports.Application.ConfirmRecipeImportDraft;

/// <summary>
/// Команда подтверждения черновика и создания рецепта.
/// </summary>
public sealed record ConfirmRecipeImportDraftCommand(
    Guid DraftId,
    CreateRecipeRequest Request) : ICommand<RecipeResponse>;
