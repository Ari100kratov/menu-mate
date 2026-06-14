using MenuMate.SharedKernel;

namespace MenuMate.Modules.RecipeImports.Domain.Errors;

/// <summary>
/// Доменные ошибки черновика импорта рецепта.
/// </summary>
public static class RecipeImportDraftErrors
{
    /// <summary>Подтвержденный черновик нельзя редактировать.</summary>
    public static readonly AppError AlreadyConfirmed = AppError.Conflict(
        "Imports.AlreadyConfirmed",
        "Подтвержденный черновик импорта нельзя изменить.");
}
