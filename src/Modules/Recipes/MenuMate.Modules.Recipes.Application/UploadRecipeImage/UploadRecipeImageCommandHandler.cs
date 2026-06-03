using MenuMate.Common.Application;
using MenuMate.Common.Application.Storage;
using MenuMate.Contracts.Recipes;
using MenuMate.Modules.Recipes.Application.Abstractions;
using MenuMate.Modules.Recipes.Application.RecipeImages;
using MenuMate.Modules.Recipes.Domain.Enums;
using MenuMate.Modules.Recipes.Domain.Models;
using MenuMate.SharedKernel;
using Microsoft.Extensions.Logging;

namespace MenuMate.Modules.Recipes.Application.UploadRecipeImage;

internal sealed class UploadRecipeImageCommandHandler(
    IRecipesRepository recipesRepository,
    IRecipeImagesRepository recipeImagesRepository,
    IRecipesUnitOfWork unitOfWork,
    IUserContext userContext,
    IObjectStorageService objectStorageService,
    RecipeImageStorageOptions storageOptions,
    TimeProvider timeProvider,
    ILogger<UploadRecipeImageCommandHandler> logger)
    : ICommandHandler<UploadRecipeImageCommand, RecipeImageResponse>
{
    private static readonly Dictionary<string, string> FileExtensionsByContentType =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["image/jpeg"] = ".jpg",
            ["image/png"] = ".png",
            ["image/webp"] = ".webp",
            ["image/avif"] = ".avif"
        };

    private static readonly Action<ILogger, string, Exception?> LogImageObjectCleanupFailed =
        LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(1, nameof(LogImageObjectCleanupFailed)),
            "Не удалось удалить объект изображения {ObjectKey} из MinIO.");

    public async Task<Result<RecipeImageResponse>> Handle(
        UploadRecipeImageCommand command,
        CancellationToken cancellationToken)
    {
        Result<RecipeImageScope> scope = ParseScope(command.Scope);
        if (scope.IsFailure)
        {
            return Result.Failure<RecipeImageResponse>(scope.Error);
        }

        Result validation = ValidateFile(command, scope.Value);
        if (validation.IsFailure)
        {
            return Result.Failure<RecipeImageResponse>(validation.Error);
        }

        Recipe? recipe = await recipesRepository.GetByIdAsync(command.RecipeId, cancellationToken);
        if (recipe is null)
        {
            return Result.Failure<RecipeImageResponse>(RecipeApplicationErrors.NotFound(command.RecipeId));
        }

        if (recipe.OwnerUserId != userContext.UserId)
        {
            return Result.Failure<RecipeImageResponse>(RecipeApplicationErrors.AccessDenied);
        }

        if (scope.Value == RecipeImageScope.Step && recipe.Steps.All(step => step.Number != command.StepNumber))
        {
            return Result.Failure<RecipeImageResponse>(RecipeApplicationErrors.StepImageTargetNotFound);
        }

        string extension = FileExtensionsByContentType[command.ContentType];
        var imageId = Guid.CreateVersion7();
        string objectKey = CreateObjectKey(
            userContext.UserId.Value,
            command.RecipeId,
            scope.Value,
            command.StepNumber,
            imageId,
            extension);
        DateTimeOffset now = timeProvider.GetUtcNow();
        var image = new RecipeImageMetadata(
            imageId,
            userContext.UserId,
            command.RecipeId,
            scope.Value,
            scope.Value == RecipeImageScope.Step ? command.StepNumber : null,
            storageOptions.BucketName,
            objectKey,
            command.ContentType,
            command.ContentLength,
            Path.GetFileName(command.FileName),
            NormalizeOptionalText(command.AltText),
            now);

        try
        {
            await objectStorageService.EnsureBucketExistsAsync(storageOptions.BucketName, cancellationToken);
            await objectStorageService.PutObjectAsync(
                storageOptions.BucketName,
                objectKey,
                command.Content,
                command.ContentLength,
                command.ContentType,
                cancellationToken);

            IReadOnlyCollection<RecipeImageObjectReference> replacedImages = [];
            if (scope.Value == RecipeImageScope.Cover)
            {
                replacedImages = await recipeImagesRepository.MarkActiveImagesDeletedAsync(
                    command.RecipeId,
                    userContext.UserId,
                    RecipeImageScope.Cover,
                    stepNumber: null,
                    cancellationToken);
            }

            await recipeImagesRepository.AddAsync(image, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            foreach (RecipeImageObjectReference replacedImage in replacedImages)
            {
                await TryDeleteObjectAsync(replacedImage.BucketName, replacedImage.ObjectKey, cancellationToken);
            }
        }
        catch (ObjectStorageException exception)
        {
            return Result.Failure<RecipeImageResponse>(RecipeApplicationErrors.ImageStorageFailed(exception.Message));
        }
        catch
        {
            await TryDeleteObjectAsync(storageOptions.BucketName, objectKey, cancellationToken);
            throw;
        }

        string readUrl = await objectStorageService.GetReadUrlAsync(
            storageOptions.BucketName,
            objectKey,
            storageOptions.ReadUrlLifetime);

        return image.ToResponse(new Uri(readUrl, UriKind.Absolute));
    }

    private static Result<RecipeImageScope> ParseScope(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return RecipeImageScope.Cover;
        }

        if (!Enum.TryParse(value, ignoreCase: true, out RecipeImageScope scope) ||
            scope == RecipeImageScope.None)
        {
            return Result.Failure<RecipeImageScope>(RecipeApplicationErrors.InvalidImageScope);
        }

        return scope;
    }

    private Result ValidateFile(UploadRecipeImageCommand command, RecipeImageScope scope)
    {
        if (command.ContentLength <= 0)
        {
            return Result.Failure(RecipeApplicationErrors.EmptyImageFile);
        }

        if (command.ContentLength > storageOptions.MaxImageSizeBytes)
        {
            return Result.Failure(RecipeApplicationErrors.ImageTooLarge(storageOptions.MaxImageSizeBytes));
        }

        if (!FileExtensionsByContentType.ContainsKey(command.ContentType))
        {
            return Result.Failure(RecipeApplicationErrors.UnsupportedImageContentType);
        }

        if (scope == RecipeImageScope.Cover && command.StepNumber is not null)
        {
            return Result.Failure(RecipeApplicationErrors.CoverImageCannotHaveStep);
        }

        if (scope == RecipeImageScope.Step && command.StepNumber is null)
        {
            return Result.Failure(RecipeApplicationErrors.StepImageRequiresStepNumber);
        }

        return Result.Success();
    }

    private static string CreateObjectKey(
        Guid ownerUserId,
        Guid recipeId,
        RecipeImageScope scope,
        int? stepNumber,
        Guid imageId,
        string extension)
    {
        string ownerSegment = ownerUserId.ToString("N");
        string recipeSegment = recipeId.ToString("N");
        string imageSegment = imageId.ToString("N");

        return scope == RecipeImageScope.Step
            ? $"users/{ownerSegment}/recipes/{recipeSegment}/images/steps/{stepNumber}/{imageSegment}{extension}"
            : $"users/{ownerSegment}/recipes/{recipeSegment}/images/cover/{imageSegment}{extension}";
    }

    private static string? NormalizeOptionalText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }

    private async Task TryDeleteObjectAsync(string bucketName, string objectKey, CancellationToken cancellationToken)
    {
        try
        {
            await objectStorageService.DeleteObjectAsync(bucketName, objectKey, cancellationToken);
        }
        catch (ObjectStorageException exception)
        {
            LogImageObjectCleanupFailed(logger, objectKey, exception);
        }
    }
}
