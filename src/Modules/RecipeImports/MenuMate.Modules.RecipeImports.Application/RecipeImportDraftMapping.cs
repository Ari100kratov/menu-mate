using MenuMate.Common.Application.Storage;
using MenuMate.Contracts.RecipeImports;
using MenuMate.Modules.RecipeImports.Domain.Models;

namespace MenuMate.Modules.RecipeImports.Application;

/// <summary>
/// Формирует внешние представления черновиков импорта.
/// </summary>
public sealed class RecipeImportDraftMapping(
    IObjectStorageService objectStorageService,
    RecipeImportStorageOptions storageOptions)
{
    /// <summary>Формирует карточку черновика для списка.</summary>
    public static RecipeImportDraftListItemResponse ToListItem(RecipeImportDraft draft)
    {
        ArgumentNullException.ThrowIfNull(draft);
        return new(
            draft.Id.Value,
            draft.Status.ToString(),
            draft.Title,
            draft.CreatedRecipeId?.Value,
            draft.CreatedAt,
            draft.UpdatedAt);
    }

    /// <summary>Формирует полный ответ с временной ссылкой на источник.</summary>
    public async Task<RecipeImportDraftResponse> ToResponseAsync(
        RecipeImportDraft draft,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(draft);
        cancellationToken.ThrowIfCancellationRequested();
        RecipeImportSourceImageResponse[] sourceImages = await Task.WhenAll(
            draft.SourceImages.Select(async image =>
            {
                string readUrl = await objectStorageService.GetReadUrlAsync(
                    image.BucketName,
                    image.ObjectKey,
                    storageOptions.ReadUrlLifetime);
                return new RecipeImportSourceImageResponse(
                    new Uri(readUrl, UriKind.Absolute),
                    image.ContentType,
                    image.SizeBytes,
                    image.FileName);
            }));

        return new RecipeImportDraftResponse(
            draft.Id.Value,
            draft.Status.ToString(),
            RecipeImportJson.DeserializeRecipe(draft.RecipeJson),
            RecipeImportJson.DeserializeEvidence(draft.EvidenceJson),
            sourceImages,
            draft.CreatedRecipeId?.Value,
            draft.CreatedAt,
            draft.UpdatedAt);
    }
}
