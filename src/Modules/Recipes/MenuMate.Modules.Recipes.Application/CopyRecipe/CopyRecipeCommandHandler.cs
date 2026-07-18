using MenuMate.Common.Application;
using MenuMate.Common.Application.Storage;
using MenuMate.Common.Application.Tags;
using MenuMate.Contracts.Recipes;
using MenuMate.Modules.Recipes.Application.Abstractions;
using MenuMate.Modules.Recipes.Application.UploadRecipeImage;
using MenuMate.Modules.Recipes.Domain.Enums;
using MenuMate.Modules.Recipes.Domain.Models;
using MenuMate.Modules.Recipes.Domain.ValueObjects;
using MenuMate.SharedKernel;
using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.Recipes.Application.CopyRecipe;

internal sealed class CopyRecipeCommandHandler(
    IRecipesRepository repository,
    IRecipeImagesRepository imageRepository,
    IRecipesReadDbContext readDbContext,
    IRecipesUnitOfWork unitOfWork,
    IUserContext userContext,
    TimeProvider timeProvider,
    RecipeProductResolver productResolver,
    RecipeTagCatalogResolver tagCatalogResolver,
    IObjectStorageService objectStorageService,
    RecipeImageStorageOptions imageStorageOptions)
    : ICommandHandler<CopyRecipeCommand, RecipeResponse>
{
    public async Task<Result<RecipeResponse>> Handle(
        CopyRecipeCommand command,
        CancellationToken cancellationToken)
    {
        RecipeRevisionAccessReadModel? access = await readDbContext.GetRevisionAccessAsync(
            command.RecipeId,
            command.Request.SourceRevisionId,
            userContext.UserId,
            cancellationToken);
        if (access is null)
        {
            return Result.Failure<RecipeResponse>(RecipeApplicationErrors.AccessDenied);
        }

        Result<RecipeDraft> draft = RecipeRequestMapper.Map(command.Request.Recipe);
        if (draft.IsFailure)
        {
            return Result.Failure<RecipeResponse>(draft.Error);
        }

        Result<IReadOnlyCollection<RecipeIngredient>> ingredients = await productResolver.ResolveAsync(
            draft.Value.Ingredients,
            cancellationToken);
        if (ingredients.IsFailure)
        {
            return Result.Failure<RecipeResponse>(ingredients.Error);
        }

        Result<IReadOnlyCollection<RecipeTag>> tags = await tagCatalogResolver.ResolveAsync(
            draft.Value.Tags,
            TagCatalogSource.User,
            cancellationToken);
        if (tags.IsFailure)
        {
            return Result.Failure<RecipeResponse>(tags.Error);
        }

        DateTimeOffset now = timeProvider.GetUtcNow();
        var sourceRevisionId = RecipeRevisionId.From(command.Request.SourceRevisionId);
        var copy = Recipe.Create(
            Guid.CreateVersion7(),
            userContext.UserId,
            draft.Value.Title,
            draft.Value.Servings,
            draft.Value.Category,
            draft.Value.Visibility,
            now,
            command.RecipeId,
            sourceRevisionId);
        copy.UpdateDetails(
            draft.Value.Title,
            draft.Value.Servings,
            draft.Value.Category,
            draft.Value.Visibility,
            draft.Value.TotalTimeMinutes,
            draft.Value.ActiveTimeMinutes,
            draft.Value.Description,
            draft.Value.SourceUrl,
            now);
        copy.ReplaceIngredients(ingredients.Value, now);
        copy.ReplaceSteps(draft.Value.Steps, now);
        copy.ReplaceTags(tags.Value, now);

        RecipeImageMetadata? copiedCover = null;
        try
        {
            if (command.Request.CopySourceCover && access.IsSourceAccessible)
            {
                RecipeImageMetadata? sourceCover = await imageRepository.GetActiveCoverAsync(
                    command.RecipeId,
                    cancellationToken);
                if (sourceCover is not null)
                {
                    copiedCover = await CopyCoverAsync(sourceCover, copy.Id, now, cancellationToken);
                }
            }

            await repository.AddAsync(copy, cancellationToken);
            if (copiedCover is not null)
            {
                await imageRepository.AddAsync(copiedCover, cancellationToken);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (ObjectStorageException exception)
        {
            if (copiedCover is not null)
            {
                await TryDeleteCopiedCoverAsync(copiedCover, cancellationToken);
            }

            return Result.Failure<RecipeResponse>(
                RecipeApplicationErrors.ImageStorageFailed(exception.Message));
        }
        catch
        {
            if (copiedCover is not null)
            {
                await TryDeleteCopiedCoverAsync(copiedCover, cancellationToken);
            }

            throw;
        }

        return RecipeMapping.ToResponse(copy, userContext.UserId);
    }

    private async Task<RecipeImageMetadata> CopyCoverAsync(
        RecipeImageMetadata source,
        Guid targetRecipeId,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var imageId = Guid.CreateVersion7();
        string extension = Path.GetExtension(source.ObjectKey);
        string objectKey = CreateCoverObjectKey(userContext.UserId.Value, targetRecipeId, imageId, extension);

        await objectStorageService.EnsureBucketExistsAsync(imageStorageOptions.BucketName, cancellationToken);
        await using Stream content = await objectStorageService.GetObjectStreamAsync(
            source.BucketName,
            source.ObjectKey,
            cancellationToken);
        await objectStorageService.PutObjectAsync(
            imageStorageOptions.BucketName,
            objectKey,
            content,
            source.SizeBytes,
            source.ContentType,
            cancellationToken);

        return new RecipeImageMetadata(
            imageId,
            userContext.UserId,
            targetRecipeId,
            RecipeImageScope.Cover,
            null,
            imageStorageOptions.BucketName,
            objectKey,
            source.ContentType,
            source.SizeBytes,
            source.OriginalFileName,
            source.AltText,
            source.SourceUrl,
            source.AuthorName,
            source.LicenseName,
            source.LicenseUrl,
            now);
    }

    private static string CreateCoverObjectKey(
        Guid ownerUserId,
        Guid recipeId,
        Guid imageId,
        string extension) =>
        $"users/{ownerUserId:N}/recipes/{recipeId:N}/images/cover/{imageId:N}{extension}";

    private async Task TryDeleteCopiedCoverAsync(
        RecipeImageMetadata cover,
        CancellationToken cancellationToken)
    {
        try
        {
            await objectStorageService.DeleteObjectAsync(
                cover.BucketName,
                cover.ObjectKey,
                cancellationToken);
        }
        catch (ObjectStorageException)
        {
            // The failed copy remains unreachable and can be removed by storage maintenance.
        }
    }
}
