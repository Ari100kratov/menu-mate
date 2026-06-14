using MenuMate.Modules.RecipeImports.Domain.Models;
using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.RecipeImports.Application.Abstractions;

internal interface IRecipeImportDraftRepository
{
    Task<RecipeImportDraft?> GetByIdAsync(ImportDraftId id, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<RecipeImportDraft>> GetRecentAsync(
        UserId ownerUserId,
        int limit,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<RecipeImportDraft>> GetExpiredAsync(
        DateTimeOffset cutoff,
        int limit,
        CancellationToken cancellationToken);

    Task AddAsync(RecipeImportDraft draft, CancellationToken cancellationToken);

    Task UpdateAsync(RecipeImportDraft draft, CancellationToken cancellationToken);

    Task DeleteAsync(RecipeImportDraft draft, CancellationToken cancellationToken);
}
