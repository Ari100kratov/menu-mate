using MenuMate.Common.Application;
using MenuMate.Common.Application.Storage;
using MenuMate.Modules.RecipeImports.Domain.Models;
using MenuMate.Modules.RecipeImports.Infrastructure.Database.Entities;
using MenuMate.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace MenuMate.Modules.RecipeImports.Infrastructure.Database;

internal sealed class RecipeImportsUserDataEraser(
    RecipeImportsDbContext dbContext,
    IObjectStorageService objectStorageService) : IUserDataEraser
{
    public int Order => 100;

    public async Task EraseAsync(UserId userId, CancellationToken cancellationToken)
    {
        RecipeImportDraftRecord[] records = await dbContext.RecipeImportDrafts
            .AsNoTracking()
            .Where(draft => draft.OwnerUserId == userId)
            .ToArrayAsync(cancellationToken);
        RecipeImportDraft[] drafts = [.. records.Select(record => record.ToDomain())];

        foreach (RecipeImportSourceImage image in drafts.SelectMany(draft => draft.SourceImages))
        {
            await objectStorageService.DeleteObjectAsync(image.BucketName, image.ObjectKey, cancellationToken);
        }

        await dbContext.RecipeImportDrafts
            .Where(draft => draft.OwnerUserId == userId)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
