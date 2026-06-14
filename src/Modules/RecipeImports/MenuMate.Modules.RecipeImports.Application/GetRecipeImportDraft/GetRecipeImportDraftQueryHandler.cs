using MenuMate.Common.Application;
using MenuMate.Contracts.RecipeImports;
using MenuMate.Modules.RecipeImports.Application.Abstractions;
using MenuMate.Modules.RecipeImports.Domain.Models;
using MenuMate.SharedKernel;
using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.RecipeImports.Application.GetRecipeImportDraft;

internal sealed class GetRecipeImportDraftQueryHandler(
    IRecipeImportDraftRepository repository,
    RecipeImportDraftMapping mapping,
    IUserContext userContext)
    : IQueryHandler<GetRecipeImportDraftQuery, RecipeImportDraftResponse>
{
    public async Task<Result<RecipeImportDraftResponse>> Handle(
        GetRecipeImportDraftQuery query,
        CancellationToken cancellationToken)
    {
        RecipeImportDraft? draft = await repository.GetByIdAsync(
            ImportDraftId.From(query.DraftId),
            cancellationToken);
        if (draft is null)
        {
            return Result.Failure<RecipeImportDraftResponse>(ImportApplicationErrors.NotFound(query.DraftId));
        }

        if (draft.OwnerUserId != userContext.UserId)
        {
            return Result.Failure<RecipeImportDraftResponse>(ImportApplicationErrors.AccessDenied);
        }

        return await mapping.ToResponseAsync(draft, cancellationToken);
    }
}
