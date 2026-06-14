using MenuMate.Modules.RecipeImports.Application.Abstractions;
using MenuMate.Modules.RecipeImports.Domain.Models;
using MenuMate.Modules.RecipeImports.Infrastructure.Database.Entities;
using MenuMate.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace MenuMate.Modules.RecipeImports.Infrastructure.Database;

internal sealed class EfRecipeImportDraftRepository(RecipeImportsDbContext dbContext) : IRecipeImportDraftRepository
{
    public async Task<RecipeImportDraft?> GetByIdAsync(
        ImportDraftId id,
        CancellationToken cancellationToken)
    {
        RecipeImportDraftRecord? record = await dbContext.RecipeImportDrafts
            .AsNoTracking()
            .SingleOrDefaultAsync(draft => draft.Id == id, cancellationToken);
        return record?.ToDomain();
    }

    public async Task<IReadOnlyCollection<RecipeImportDraft>> GetRecentAsync(
        UserId ownerUserId,
        int limit,
        CancellationToken cancellationToken)
    {
        RecipeImportDraftRecord[] records = await dbContext.RecipeImportDrafts
            .AsNoTracking()
            .Where(draft => draft.OwnerUserId == ownerUserId)
            .OrderByDescending(draft => draft.UpdatedAt)
            .Take(limit)
            .ToArrayAsync(cancellationToken);
        return records.Select(record => record.ToDomain()).ToArray();
    }

    public async Task<IReadOnlyCollection<RecipeImportDraft>> GetExpiredAsync(
        DateTimeOffset cutoff,
        int limit,
        CancellationToken cancellationToken)
    {
        RecipeImportDraftRecord[] records = await dbContext.RecipeImportDrafts
            .AsNoTracking()
            .Where(draft => draft.UpdatedAt < cutoff)
            .OrderBy(draft => draft.UpdatedAt)
            .Take(limit)
            .ToArrayAsync(cancellationToken);
        return records.Select(record => record.ToDomain()).ToArray();
    }

    public Task AddAsync(RecipeImportDraft draft, CancellationToken cancellationToken) =>
        dbContext.RecipeImportDrafts.AddAsync(
            RecipeImportDraftRecord.FromDomain(draft),
            cancellationToken).AsTask();

    public async Task UpdateAsync(RecipeImportDraft draft, CancellationToken cancellationToken)
    {
        RecipeImportDraftRecord? record = await dbContext.RecipeImportDrafts
            .SingleOrDefaultAsync(existing => existing.Id == draft.Id, cancellationToken);
        record?.Apply(draft);
    }

    public async Task DeleteAsync(RecipeImportDraft draft, CancellationToken cancellationToken)
    {
        RecipeImportDraftRecord? record = await dbContext.RecipeImportDrafts
            .SingleOrDefaultAsync(existing => existing.Id == draft.Id, cancellationToken);
        if (record is not null)
        {
            dbContext.RecipeImportDrafts.Remove(record);
        }
    }
}
