using MenuMate.Common.Application;
using MenuMate.Contracts.RecipeImports;

namespace MenuMate.Modules.RecipeImports.Application.CreateRecipeImportDraft;

/// <summary>
/// Команда создания черновика импорта из изображения.
/// </summary>
public sealed record CreateRecipeImportDraftCommand(
    IReadOnlyCollection<RecipeImportSourceFile> Files) : ICommand<RecipeImportDraftResponse>;

/// <summary>Исходный файл рецепта.</summary>
public sealed record RecipeImportSourceFile(
    Stream Content,
    string FileName,
    string ContentType,
    long ContentLength);
