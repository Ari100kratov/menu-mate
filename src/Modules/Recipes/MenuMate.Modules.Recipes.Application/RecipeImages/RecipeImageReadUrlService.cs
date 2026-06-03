using MenuMate.Common.Application.Storage;
using MenuMate.Contracts.Recipes;
using MenuMate.Modules.Recipes.Application.UploadRecipeImage;

namespace MenuMate.Modules.Recipes.Application.RecipeImages;

/// <summary>
/// Добавляет к metadata изображений временные или публичные ссылки чтения из MinIO.
/// </summary>
internal sealed class RecipeImageReadUrlService(
    IObjectStorageService objectStorageService,
    RecipeImageStorageOptions imageStorageOptions)
{
    public async Task<RecipeImageResponse> AddReadUrlAsync(RecipeImageResponse image)
    {
        string readUrl = await objectStorageService.GetReadUrlAsync(
            image.BucketName,
            image.ObjectKey,
            imageStorageOptions.ReadUrlLifetime);

        return image with { ReadUrl = new Uri(readUrl, UriKind.Absolute) };
    }

    public async Task<IReadOnlyCollection<RecipeImageResponse>> AddReadUrlsAsync(
        IReadOnlyCollection<RecipeImageResponse> images)
    {
        if (images.Count == 0)
        {
            return images;
        }

        var imagesWithReadUrls = new List<RecipeImageResponse>(images.Count);
        foreach (RecipeImageResponse image in images)
        {
            imagesWithReadUrls.Add(await AddReadUrlAsync(image));
        }

        return imagesWithReadUrls;
    }
}
