using MenuMate.Common.Application;
using MenuMate.Contracts.RecipeImports;
using MenuMate.Modules.RecipeImports.Application.Abstractions;
using MenuMate.Modules.RecipeImports.Domain.Models;
using MenuMate.SharedKernel;
using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.RecipeImports.Application.UpdateRecipeImportDraft;

internal sealed class UpdateRecipeImportDraftCommandHandler(
    IRecipeImportDraftRepository repository,
    IRecipeImportsUnitOfWork unitOfWork,
    RecipeImportDraftMapping mapping,
    IUserContext userContext,
    TimeProvider timeProvider)
    : ICommandHandler<UpdateRecipeImportDraftCommand, RecipeImportDraftResponse>
{
    public async Task<Result<RecipeImportDraftResponse>> Handle(
        UpdateRecipeImportDraftCommand command,
        CancellationToken cancellationToken)
    {
        RecipeImportDraft? draft = await repository.GetByIdAsync(
            ImportDraftId.From(command.DraftId),
            cancellationToken);
        if (draft is null)
        {
            return Result.Failure<RecipeImportDraftResponse>(ImportApplicationErrors.NotFound(command.DraftId));
        }

        if (draft.OwnerUserId != userContext.UserId)
        {
            return Result.Failure<RecipeImportDraftResponse>(ImportApplicationErrors.AccessDenied);
        }

        Result update = draft.Update(
            command.Request.Recipe.Title,
            RecipeImportJson.SerializeRecipe(command.Request.Recipe),
            timeProvider.GetUtcNow());
        if (update.IsFailure)
        {
            return Result.Failure<RecipeImportDraftResponse>(update.Error);
        }

        await repository.UpdateAsync(draft, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return await mapping.ToResponseAsync(draft, cancellationToken);
    }
}
