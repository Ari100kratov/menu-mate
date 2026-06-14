namespace MenuMate.Modules.RecipeImports.Application;

/// <summary>
/// Настройки хранения исходных изображений импорта.
/// </summary>
public sealed class RecipeImportStorageOptions
{
    /// <summary>Имя приватного бакета.</summary>
    public string BucketName { get; init; } = "imports";

    /// <summary>Максимальный размер изображения.</summary>
    public long MaxImageSizeBytes { get; init; } = 10 * 1024 * 1024;

    /// <summary>Максимальное количество изображений одного рецепта.</summary>
    public int MaxImageCount { get; init; } = 8;

    /// <summary>Максимальный суммарный размер изображений одного рецепта.</summary>
    public long MaxTotalImageSizeBytes { get; init; } = 40 * 1024 * 1024;

    /// <summary>Время жизни временной ссылки.</summary>
    public TimeSpan ReadUrlLifetime { get; init; } = TimeSpan.FromHours(1);

    /// <summary>Время хранения черновика после последнего изменения.</summary>
    public TimeSpan DraftRetentionPeriod { get; init; } = TimeSpan.FromDays(7);

    /// <summary>Интервал фоновой очистки просроченных черновиков.</summary>
    public TimeSpan CleanupInterval { get; init; } = TimeSpan.FromHours(1);

    /// <summary>Максимальное количество черновиков в одной порции очистки.</summary>
    public int CleanupBatchSize { get; init; } = 100;
}
