using MenuMate.Common.Application;
using MenuMate.Modules.Recipes.Application.Abstractions;
using MenuMate.Modules.Recipes.Domain.Models;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.Recipes.Application.DeleteRecipe;

internal sealed class DeleteRecipeCommandHandler(
    IRecipesRepository repository,
    IRecipesUnitOfWork unitOfWork,
    IUserContext userContext,
    TimeProvider timeProvider)
    : ICommandHandler<DeleteRecipeCommand>
{
    public async Task<Result> Handle(DeleteRecipeCommand command, CancellationToken cancellationToken)
    {
        Recipe? recipe = await repository.GetByIdAsync(command.RecipeId, cancellationToken);
        if (recipe is null)
        {
            return Result.Failure(RecipeApplicationErrors.NotFound(command.RecipeId));
        }

        if (recipe.OwnerUserId != userContext.UserId)
        {
            return Result.Failure(RecipeApplicationErrors.AccessDenied);
        }

        recipe.Delete(timeProvider.GetUtcNow());
        await repository.UpdateAsync(recipe, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
