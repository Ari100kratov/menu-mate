using MenuMate.Modules.RecipeImports.Domain.Enums;
using MenuMate.Modules.RecipeImports.Domain.Errors;
using MenuMate.SharedKernel;
using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.RecipeImports.Domain.Models;

/// <summary>
/// Сохраненный результат распознавания рецепта, ожидающий подтверждения пользователя.
/// </summary>
public sealed class RecipeImportDraft : Entity<ImportDraftId>
{
    private RecipeImportDraft(
        ImportDraftId id,
        UserId ownerUserId,
        RecipeId targetRecipeId,
        RecipeImportDraftStatus status,
        string title,
        string recipeJson,
        string evidenceJson,
        string bucketName,
        string objectKey,
        string contentType,
        long sizeBytes,
        string fileName,
        IReadOnlyCollection<RecipeImportSourceImage> additionalSourceImages,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        RecipeId? createdRecipeId)
        : base(id)
    {
        OwnerUserId = ownerUserId;
        TargetRecipeId = targetRecipeId;
        Status = status;
        Title = title;
        RecipeJson = recipeJson;
        EvidenceJson = evidenceJson;
        BucketName = bucketName;
        ObjectKey = objectKey;
        ContentType = contentType;
        SizeBytes = sizeBytes;
        FileName = fileName;
        AdditionalSourceImages = additionalSourceImages;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        CreatedRecipeId = createdRecipeId;
    }

    /// <summary>Владелец черновика.</summary>
    public UserId OwnerUserId { get; }

    /// <summary>Заранее назначенный идентификатор будущего рецепта.</summary>
    public RecipeId TargetRecipeId { get; }

    /// <summary>Текущее состояние.</summary>
    public RecipeImportDraftStatus Status { get; private set; }

    /// <summary>Заголовок для списка черновиков.</summary>
    public string Title { get; private set; }

    /// <summary>JSON-снимок редактируемого рецепта.</summary>
    public string RecipeJson { get; private set; }

    /// <summary>JSON-снимок доказательств распознавания.</summary>
    public string EvidenceJson { get; }

    /// <summary>Бакет исходного изображения.</summary>
    public string BucketName { get; }

    /// <summary>Ключ исходного изображения.</summary>
    public string ObjectKey { get; }

    /// <summary>MIME-тип исходного изображения.</summary>
    public string ContentType { get; }

    /// <summary>Размер исходного изображения.</summary>
    public long SizeBytes { get; }

    /// <summary>Исходное имя файла.</summary>
    public string FileName { get; }

    /// <summary>Дополнительные исходные изображения.</summary>
    public IReadOnlyCollection<RecipeImportSourceImage> AdditionalSourceImages { get; }

    /// <summary>Все исходные изображения в порядке загрузки.</summary>
    public IReadOnlyCollection<RecipeImportSourceImage> SourceImages =>
        [
            new(BucketName, ObjectKey, ContentType, SizeBytes, FileName),
            .. AdditionalSourceImages
        ];

    /// <summary>Дата создания.</summary>
    public DateTimeOffset CreatedAt { get; }

    /// <summary>Дата последнего изменения.</summary>
    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>Созданный рецепт.</summary>
    public RecipeId? CreatedRecipeId { get; private set; }

    /// <summary>Создает новый черновик.</summary>
    public static RecipeImportDraft Create(
        ImportDraftId id,
        UserId ownerUserId,
        RecipeId targetRecipeId,
        string title,
        string recipeJson,
        string evidenceJson,
        string bucketName,
        string objectKey,
        string contentType,
        long sizeBytes,
        string fileName,
        IReadOnlyCollection<RecipeImportSourceImage> additionalSourceImages,
        DateTimeOffset now) =>
        new(
            id,
            ownerUserId,
            targetRecipeId,
            RecipeImportDraftStatus.Ready,
            NormalizeTitle(title),
            recipeJson,
            evidenceJson,
            bucketName,
            objectKey,
            contentType,
            sizeBytes,
            fileName,
            additionalSourceImages,
            now,
            now,
            createdRecipeId: null);

    /// <summary>Восстанавливает черновик из persistence.</summary>
    public static RecipeImportDraft Rehydrate(
        ImportDraftId id,
        UserId ownerUserId,
        RecipeId targetRecipeId,
        RecipeImportDraftStatus status,
        string title,
        string recipeJson,
        string evidenceJson,
        string bucketName,
        string objectKey,
        string contentType,
        long sizeBytes,
        string fileName,
        IReadOnlyCollection<RecipeImportSourceImage> additionalSourceImages,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        RecipeId? createdRecipeId) =>
        new(
            id,
            ownerUserId,
            targetRecipeId,
            status,
            title,
            recipeJson,
            evidenceJson,
            bucketName,
            objectKey,
            contentType,
            sizeBytes,
            fileName,
            additionalSourceImages,
            createdAt,
            updatedAt,
            createdRecipeId);

    /// <summary>Заменяет редактируемый снимок черновика.</summary>
    public Result Update(string title, string recipeJson, DateTimeOffset now)
    {
        if (Status == RecipeImportDraftStatus.Confirmed)
        {
            return Result.Failure(RecipeImportDraftErrors.AlreadyConfirmed);
        }

        Title = NormalizeTitle(title);
        RecipeJson = recipeJson;
        UpdatedAt = now;
        return Result.Success();
    }

    /// <summary>Отмечает черновик подтвержденным.</summary>
    public void Confirm(RecipeId recipeId, DateTimeOffset now)
    {
        Status = RecipeImportDraftStatus.Confirmed;
        CreatedRecipeId = recipeId;
        UpdatedAt = now;
    }

    private static string NormalizeTitle(string value) =>
        string.IsNullOrWhiteSpace(value) ? "Рецепт без названия" : value.Trim();
}
