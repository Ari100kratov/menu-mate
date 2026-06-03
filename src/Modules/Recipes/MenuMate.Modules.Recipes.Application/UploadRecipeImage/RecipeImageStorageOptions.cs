namespace MenuMate.Modules.Recipes.Application.UploadRecipeImage;

/// <summary>
/// Настройки хранения изображений рецептов.
/// </summary>
internal sealed class RecipeImageStorageOptions
{
    /// <summary>
    /// Имя бакета MinIO для пользовательских изображений.
    /// </summary>
    public string BucketName { get; init; } = "images";

    /// <summary>
    /// Максимальный размер одного изображения в байтах.
    /// </summary>
    public long MaxImageSizeBytes { get; init; } = 10 * 1024 * 1024;

    /// <summary>
    /// Время жизни ссылки для прямого чтения изображения.
    /// </summary>
    public TimeSpan ReadUrlLifetime { get; init; } = TimeSpan.FromHours(1);
}
