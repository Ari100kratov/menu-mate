using MenuMate.SharedKernel;

namespace MenuMate.Modules.Recipes.Application;

/// <summary>
/// Ошибки прикладного слоя рецептов.
/// </summary>
public static class RecipeApplicationErrors
{
    /// <summary>
    /// Рецепт не найден.
    /// </summary>
    public static AppError NotFound(Guid recipeId) => AppError.NotFound(
        "Recipes.NotFound",
        $"Recipe '{recipeId}' was not found.");

    /// <summary>
    /// Текущий пользователь не может получить доступ к рецепту.
    /// </summary>
    public static readonly AppError AccessDenied = AppError.Forbidden(
        "Recipes.AccessDenied",
        "Current user cannot access this recipe.");

    /// <summary>
    /// Область привязки изображения указана в неизвестном формате.
    /// </summary>
    public static readonly AppError InvalidImageScope = AppError.Validation(
        "Recipes.InvalidImageScope",
        "Область изображения должна быть Cover или Step.");

    /// <summary>
    /// Файл изображения пустой.
    /// </summary>
    public static readonly AppError EmptyImageFile = AppError.Validation(
        "Recipes.EmptyImageFile",
        "Файл изображения не должен быть пустым.");

    /// <summary>
    /// MIME-тип изображения не поддерживается.
    /// </summary>
    public static readonly AppError UnsupportedImageContentType = AppError.Validation(
        "Recipes.UnsupportedImageContentType",
        "Поддерживаются только изображения JPEG, PNG, WebP и AVIF.");

    /// <summary>
    /// Обложка рецепта не может ссылаться на шаг приготовления.
    /// </summary>
    public static readonly AppError CoverImageCannotHaveStep = AppError.Validation(
        "Recipes.CoverImageCannotHaveStep",
        "Обложка рецепта не должна ссылаться на шаг приготовления.");

    /// <summary>
    /// Изображение шага должно ссылаться на номер шага.
    /// </summary>
    public static readonly AppError StepImageRequiresStepNumber = AppError.Validation(
        "Recipes.StepImageRequiresStepNumber",
        "Для изображения шага нужно указать номер шага приготовления.");

    /// <summary>
    /// Шаг приготовления для изображения не найден.
    /// </summary>
    public static readonly AppError StepImageTargetNotFound = AppError.Validation(
        "Recipes.StepImageTargetNotFound",
        "Шаг приготовления для изображения не найден.");

    /// <summary>
    /// Изображение рецепта не найдено.
    /// </summary>
    public static AppError ImageNotFound(Guid imageId) => AppError.NotFound(
        "Recipes.ImageNotFound",
        $"Изображение рецепта '{imageId}' не найдено.");

    /// <summary>
    /// Размер изображения превышает допустимый лимит.
    /// </summary>
    public static AppError ImageTooLarge(long maxImageSizeBytes) => AppError.Validation(
        "Recipes.ImageTooLarge",
        $"Размер изображения не должен превышать {maxImageSizeBytes} байт.");

    /// <summary>
    /// Не удалось сохранить изображение в объектное хранилище.
    /// </summary>
    public static AppError ImageStorageFailed(string reason) => AppError.Problem(
        "Recipes.ImageStorageFailed",
        $"Не удалось сохранить изображение: {reason}");
}
