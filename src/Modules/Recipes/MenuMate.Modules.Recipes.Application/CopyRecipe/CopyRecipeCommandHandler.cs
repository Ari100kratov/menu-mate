using MenuMate.Common.Application;
using MenuMate.Contracts.Recipes;
using MenuMate.Modules.Recipes.Application.Abstractions;
using MenuMate.Modules.Recipes.Domain.Enums;
using MenuMate.Modules.Recipes.Domain.Models;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.Recipes.Application.CopyRecipe;

internal sealed class CopyRecipeCommandHandler(
    IRecipesRepository repository,
    IRecipesUnitOfWork unitOfWork,
    IUserContext userContext,
    TimeProvider timeProvider)
    : ICommandHandler<CopyRecipeCommand, RecipeResponse>
{
    public async Task<Result<RecipeResponse>> Handle(
        CopyRecipeCommand command,
        CancellationToken cancellationToken)
    {
        Recipe? source = await repository.GetByIdAsync(command.RecipeId, cancellationToken);
        if (source is null)
        {
            return Result.Failure<RecipeResponse>(RecipeApplicationErrors.NotFound(command.RecipeId));
        }

        if (source.OwnerUserId != userContext.UserId && source.Visibility != RecipeVisibility.Public)
        {
            return Result.Failure<RecipeResponse>(RecipeApplicationErrors.AccessDenied);
        }

        DateTimeOffset now = timeProvider.GetUtcNow();
        var copy = Recipe.Create(
            Guid.CreateVersion7(),
            userContext.UserId,
            source.Title,
            source.Servings,
            source.Category,
            RecipeVisibility.Private,
            now,
            source.Id,
            source.CurrentRevisionId);
        copy.UpdateDetails(
            source.Title,
            source.Servings,
            source.Category,
            RecipeVisibility.Private,
            source.TotalTimeMinutes,
            source.ActiveTimeMinutes,
            source.Description,
            source.SourceUrl,
            now);
        copy.ReplaceIngredients(source.Ingredients, now);
        copy.ReplaceSteps(source.Steps, now);
        copy.ReplaceTags(source.Tags, now);

        await repository.AddAsync(copy, cancellationToken);
        await repository.SaveToLibraryAsync(
            copy.Id,
            userContext.UserId,
            now,
            cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return RecipeMapping.ToResponse(copy, userContext.UserId, isSaved: true, isFavorite: false);
    }
}
