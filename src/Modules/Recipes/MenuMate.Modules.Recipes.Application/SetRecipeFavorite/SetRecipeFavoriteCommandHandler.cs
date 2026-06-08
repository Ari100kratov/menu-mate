using MenuMate.Common.Application;
using MenuMate.Modules.Recipes.Application.Abstractions;
using MenuMate.Modules.Recipes.Domain.Enums;
using MenuMate.Modules.Recipes.Domain.Models;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.Recipes.Application.SetRecipeFavorite;

internal sealed class SetRecipeFavoriteCommandHandler(
    IRecipesRepository repository,
    IRecipesUnitOfWork unitOfWork,
    IUserContext userContext,
    TimeProvider timeProvider)
    : ICommandHandler<SetRecipeFavoriteCommand>
{
    public async Task<Result> Handle(SetRecipeFavoriteCommand command, CancellationToken cancellationToken)
    {
        Recipe? recipe = await repository.GetByIdAsync(command.RecipeId, cancellationToken);
        if (recipe is null)
        {
            return Result.Failure(RecipeApplicationErrors.NotFound(command.RecipeId));
        }

        if (recipe.OwnerUserId != userContext.UserId &&
            recipe.Visibility != RecipeVisibility.Public)
        {
            return Result.Failure(RecipeApplicationErrors.AccessDenied);
        }

        DateTimeOffset now = timeProvider.GetUtcNow();
        await repository.SetFavoriteAsync(
            recipe.Id,
            userContext.UserId,
            command.IsFavorite,
            now,
            cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
