using MenuMate.Common.Application;
using MenuMate.Common.Application.Storage;
using MenuMate.Modules.Recipes.Application.Abstractions;
using MenuMate.Modules.Recipes.Application.RecipeImages;
using MenuMate.Modules.Recipes.Domain.Models;
using MenuMate.SharedKernel;
using Microsoft.Extensions.Logging;

namespace MenuMate.Modules.Recipes.Application.DeleteRecipeImage;

internal sealed class DeleteRecipeImageCommandHandler(
    IRecipesRepository recipesRepository,
    IRecipeImagesRepository recipeImagesRepository,
    IRecipesUnitOfWork unitOfWork,
    IUserContext userContext,
    IObjectStorageService objectStorageService,
    ILogger<DeleteRecipeImageCommandHandler> logger)
    : ICommandHandler<DeleteRecipeImageCommand>
{
    private static readonly Action<ILogger, string, Exception?> LogImageObjectDeleteFailed =
        LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(1, nameof(LogImageObjectDeleteFailed)),
            "Не удалось удалить объект изображения {ObjectKey} из MinIO после удаления metadata.");

    public async Task<Result> Handle(DeleteRecipeImageCommand command, CancellationToken cancellationToken)
    {
        Recipe? recipe = await recipesRepository.GetByIdAsync(command.RecipeId, cancellationToken);
        if (recipe is null)
        {
            return Result.Failure(RecipeApplicationErrors.NotFound(command.RecipeId));
        }

        if (recipe.OwnerUserId != userContext.UserId)
        {
            return Result.Failure(RecipeApplicationErrors.AccessDenied);
        }

        RecipeImageObjectReference? image = await recipeImagesRepository.MarkActiveImageDeletedAsync(
            command.RecipeId,
            command.ImageId,
            userContext.UserId,
            cancellationToken);

        if (image is null)
        {
            return Result.Failure(RecipeApplicationErrors.ImageNotFound(command.ImageId));
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await TryDeleteObjectAsync(image, cancellationToken);

        return Result.Success();
    }

    private async Task TryDeleteObjectAsync(
        RecipeImageObjectReference image,
        CancellationToken cancellationToken)
    {
        try
        {
            await objectStorageService.DeleteObjectAsync(image.BucketName, image.ObjectKey, cancellationToken);
        }
        catch (ObjectStorageException exception)
        {
            LogImageObjectDeleteFailed(logger, image.ObjectKey, exception);
        }
    }
}
