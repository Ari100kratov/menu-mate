using MenuMate.Common.Application;
using MenuMate.Modules.Recipes.Application.Abstractions;
using MenuMate.Modules.Recipes.Domain.Enums;
using MenuMate.Modules.Recipes.Domain.Models;
using MenuMate.SharedKernel;
using MenuMate.SharedKernel.Identifiers;

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
        if (!command.IsFavorite)
        {
            await repository.RemoveFromLibraryAsync(command.RecipeId, userContext.UserId, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }

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

        RecipeRevisionId revisionId = command.RevisionId.HasValue
            ? RecipeRevisionId.From(command.RevisionId.Value)
            : recipe.CurrentRevisionId;
        if (revisionId != recipe.CurrentRevisionId)
        {
            return Result.Failure(RecipeApplicationErrors.InvalidRevision(command.RecipeId, revisionId.Value));
        }

        await repository.SaveToLibraryAsync(
            recipe.Id,
            revisionId,
            userContext.UserId,
            timeProvider.GetUtcNow(),
            cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
