using MenuMate.Common.Application;
using MenuMate.Contracts.Recipes;
using MenuMate.Modules.RecipeImports.Application.Abstractions;
using MenuMate.Modules.RecipeImports.Domain.Enums;
using MenuMate.Modules.RecipeImports.Domain.Models;
using MenuMate.Modules.Recipes.Application.CreateRecipe;
using MenuMate.Modules.Recipes.Application.GetRecipeById;
using MenuMate.SharedKernel;
using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.RecipeImports.Application.ConfirmRecipeImportDraft;

internal sealed class ConfirmRecipeImportDraftCommandHandler(
    IRecipeImportDraftRepository repository,
    IRecipeImportsUnitOfWork unitOfWork,
    ICommandHandler<CreateRecipeCommand, RecipeResponse> createRecipeHandler,
    IQueryHandler<GetRecipeByIdQuery, RecipeResponse> getRecipeHandler,
    IUserContext userContext,
    TimeProvider timeProvider)
    : ICommandHandler<ConfirmRecipeImportDraftCommand, RecipeResponse>
{
    public async Task<Result<RecipeResponse>> Handle(
        ConfirmRecipeImportDraftCommand command,
        CancellationToken cancellationToken)
    {
        RecipeImportDraft? draft = await repository.GetByIdAsync(
            ImportDraftId.From(command.DraftId),
            cancellationToken);
        if (draft is null)
        {
            return Result.Failure<RecipeResponse>(ImportApplicationErrors.NotFound(command.DraftId));
        }

        if (draft.OwnerUserId != userContext.UserId)
        {
            return Result.Failure<RecipeResponse>(ImportApplicationErrors.AccessDenied);
        }

        if (draft.Status == RecipeImportDraftStatus.Confirmed && draft.CreatedRecipeId.HasValue)
        {
            return await getRecipeHandler.Handle(
                new GetRecipeByIdQuery(draft.CreatedRecipeId.Value.Value),
                cancellationToken);
        }

        Result<RecipeResponse> created = await createRecipeHandler.Handle(
            new CreateRecipeCommand(command.Request, draft.TargetRecipeId.Value),
            cancellationToken);
        if (created.IsFailure)
        {
            return created;
        }

        draft.Confirm(RecipeId.From(created.Value.Id), timeProvider.GetUtcNow());
        await repository.UpdateAsync(draft, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return created;
    }
}
