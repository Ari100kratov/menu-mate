using MenuMate.Common.Application;
using MenuMate.Contracts.RecipeImports;
using MenuMate.Modules.RecipeImports.Application.Abstractions;
using MenuMate.Modules.RecipeImports.Domain.Models;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.RecipeImports.Application.GetRecipeImportDrafts;

internal sealed class GetRecipeImportDraftsQueryHandler(
    IRecipeImportDraftRepository repository,
    IUserContext userContext)
    : IQueryHandler<GetRecipeImportDraftsQuery, IReadOnlyCollection<RecipeImportDraftListItemResponse>>
{
    public async Task<Result<IReadOnlyCollection<RecipeImportDraftListItemResponse>>> Handle(
        GetRecipeImportDraftsQuery query,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<RecipeImportDraft> drafts = await repository.GetRecentAsync(
            userContext.UserId,
            limit: 20,
            cancellationToken);
        return Result.Success<IReadOnlyCollection<RecipeImportDraftListItemResponse>>(
            drafts.Select(RecipeImportDraftMapping.ToListItem).ToArray());
    }
}
