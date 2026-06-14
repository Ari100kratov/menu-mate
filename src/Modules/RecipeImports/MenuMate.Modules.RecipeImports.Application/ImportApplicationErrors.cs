using MenuMate.SharedKernel;

namespace MenuMate.Modules.RecipeImports.Application;

/// <summary>
/// Ошибки прикладного слоя импорта.
/// </summary>
public static class ImportApplicationErrors
{
    /// <summary>Черновик не найден.</summary>
    public static AppError NotFound(Guid draftId) => AppError.NotFound(
        "Imports.NotFound",
        $"Черновик импорта '{draftId}' не найден.");

    /// <summary>Доступ к черновику запрещен.</summary>
    public static readonly AppError AccessDenied = AppError.Forbidden(
        "Imports.AccessDenied",
        "Текущий пользователь не может получить доступ к этому черновику импорта.");

    /// <summary>Файл пуст.</summary>
    public static readonly AppError EmptyImageFile = AppError.Validation(
        "Imports.EmptyImageFile",
        "Файл изображения не должен быть пустым.");

    /// <summary>Не передано ни одного изображения.</summary>
    public static readonly AppError ImagesRequired = AppError.Validation(
        "Imports.ImagesRequired",
        "Добавьте хотя бы одно изображение рецепта.");

    /// <summary>Передано слишком много изображений.</summary>
    public static AppError TooManyImages(int maxImageCount) => AppError.Validation(
        "Imports.TooManyImages",
        $"Можно загрузить не более {maxImageCount} изображений одного рецепта.");

    /// <summary>Суммарный размер изображений слишком большой.</summary>
    public static AppError ImagesTotalSizeTooLarge(long maxSizeBytes) => AppError.Validation(
        "Imports.ImagesTotalSizeTooLarge",
        $"Суммарный размер изображений не должен превышать {maxSizeBytes} байт.");

    /// <summary>Формат файла не поддерживается.</summary>
    public static readonly AppError UnsupportedImageContentType = AppError.Validation(
        "Imports.UnsupportedImageContentType",
        "Поддерживаются только изображения JPEG, PNG и WebP.");

    /// <summary>Содержимое файла не соответствует MIME-типу.</summary>
    public static readonly AppError InvalidImageSignature = AppError.Validation(
        "Imports.InvalidImageSignature",
        "Содержимое файла не соответствует заявленному формату изображения.");

    /// <summary>Файл слишком большой.</summary>
    public static AppError ImageTooLarge(long maxSizeBytes) => AppError.Validation(
        "Imports.ImageTooLarge",
        $"Размер изображения не должен превышать {maxSizeBytes} байт.");

    /// <summary>Внешняя система распознавания недоступна.</summary>
    public static AppError ExtractionFailed(string reason) => AppError.Problem(
        "Imports.ExtractionFailed",
        $"Не удалось распознать рецепт: {reason}");

    /// <summary>Внешняя система генерации обложки недоступна.</summary>
    public static AppError CoverGenerationFailed(string reason) => AppError.Problem(
        "Imports.CoverGenerationFailed",
        $"Не удалось сгенерировать обложку рецепта: {reason}");

    /// <summary>Объектное хранилище недоступно.</summary>
    public static AppError StorageFailed(string reason) => AppError.Problem(
        "Imports.StorageFailed",
        $"Не удалось сохранить исходное изображение: {reason}");
}
