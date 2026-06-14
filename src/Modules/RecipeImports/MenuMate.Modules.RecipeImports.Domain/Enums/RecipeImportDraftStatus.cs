namespace MenuMate.Modules.RecipeImports.Domain.Enums;

/// <summary>
/// Состояние черновика импорта рецепта.
/// </summary>
public enum RecipeImportDraftStatus
{
    /// <summary>Черновик готов к проверке и редактированию.</summary>
    Ready = 0,

    /// <summary>Пользователь подтвердил черновик и создал рецепт.</summary>
    Confirmed = 1
}
