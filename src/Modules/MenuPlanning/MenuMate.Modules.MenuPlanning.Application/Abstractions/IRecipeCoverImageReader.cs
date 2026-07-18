using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.MenuPlanning.Application.Abstractions;

internal interface IRecipeCoverImageReader
{
    Task<IReadOnlyDictionary<Guid, Uri>> GetReadUrlsAsync(
        UserId userId,
        IReadOnlyCollection<RecipeId> recipeIds,
        CancellationToken cancellationToken);
}
