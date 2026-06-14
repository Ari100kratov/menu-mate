using System.Text.Json;
using MenuMate.Modules.RecipeImports.Domain.Enums;
using MenuMate.Modules.RecipeImports.Domain.Models;
using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.RecipeImports.Infrastructure.Database.Entities;

internal sealed class RecipeImportDraftRecord
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public ImportDraftId Id { get; set; }

    public UserId OwnerUserId { get; set; }

    public RecipeId TargetRecipeId { get; set; }

    public RecipeImportDraftStatus Status { get; set; }

    public string Title { get; set; } = string.Empty;

    public string RecipeJson { get; set; } = string.Empty;

    public string EvidenceJson { get; set; } = string.Empty;

    public string BucketName { get; set; } = string.Empty;

    public string ObjectKey { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long SizeBytes { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string AdditionalSourceImagesJson { get; set; } = "[]";

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public RecipeId? CreatedRecipeId { get; set; }

    public static RecipeImportDraftRecord FromDomain(RecipeImportDraft draft)
    {
        var record = new RecipeImportDraftRecord();
        record.Apply(draft);
        return record;
    }

    public void Apply(RecipeImportDraft draft)
    {
        Id = draft.Id;
        OwnerUserId = draft.OwnerUserId;
        TargetRecipeId = draft.TargetRecipeId;
        Status = draft.Status;
        Title = draft.Title;
        RecipeJson = draft.RecipeJson;
        EvidenceJson = draft.EvidenceJson;
        BucketName = draft.BucketName;
        ObjectKey = draft.ObjectKey;
        ContentType = draft.ContentType;
        SizeBytes = draft.SizeBytes;
        FileName = draft.FileName;
        AdditionalSourceImagesJson = JsonSerializer.Serialize(draft.AdditionalSourceImages, JsonOptions);
        CreatedAt = draft.CreatedAt;
        UpdatedAt = draft.UpdatedAt;
        CreatedRecipeId = draft.CreatedRecipeId;
    }

    public RecipeImportDraft ToDomain() =>
        RecipeImportDraft.Rehydrate(
            Id,
            OwnerUserId,
            TargetRecipeId,
            Status,
            Title,
            RecipeJson,
            EvidenceJson,
            BucketName,
            ObjectKey,
            ContentType,
            SizeBytes,
            FileName,
            JsonSerializer.Deserialize<RecipeImportSourceImage[]>(
                AdditionalSourceImagesJson,
                JsonOptions) ?? [],
            CreatedAt,
            UpdatedAt,
            CreatedRecipeId);
}
