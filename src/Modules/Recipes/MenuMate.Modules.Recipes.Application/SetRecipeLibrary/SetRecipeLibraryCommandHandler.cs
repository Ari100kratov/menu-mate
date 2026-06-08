using MenuMate.Common.Application;
using MenuMate.Modules.Recipes.Application.Abstractions;
using MenuMate.Modules.Recipes.Domain.Enums;
using MenuMate.Modules.Recipes.Domain.Models;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.Recipes.Application.SetRecipeLibrary;

internal sealed class SetRecipeLibraryCommandHandler(
    IRecipesRepository repository,
    IRecipesUnitOfWork unitOfWork,
    IUserContext userContext,
    TimeProvider timeProvider)
    : ICommandHandler<SetRecipeLibraryCommand>
{
    public async Task<Result> Handle(SetRecipeLibraryCommand command, CancellationToken cancellationToken)
    {
        Recipe? recipe = await repository.GetByIdAsync(command.RecipeId, cancellationToken);
        if (recipe is null)
        {
            return Result.Failure(RecipeApplicationErrors.NotFound(command.RecipeId));
        }

        if (recipe.OwnerUserId != userContext.UserId && recipe.Visibility != RecipeVisibility.Public)
        {
            return Result.Failure(RecipeApplicationErrors.AccessDenied);
        }

        if (!command.IsSaved && recipe.OwnerUserId == userContext.UserId)
        {
            return Result.Success();
        }

        if (command.IsSaved)
        {
            await repository.SaveToLibraryAsync(
                recipe.Id,
                userContext.UserId,
                timeProvider.GetUtcNow(),
                cancellationToken);
        }
        else
        {
            await repository.RemoveFromLibraryAsync(recipe.Id, userContext.UserId, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
