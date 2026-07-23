using MenuMate.Common.Application.Statistics;
using MenuMate.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace MenuMate.Modules.Recipes.Infrastructure.Database;

/// <summary>
/// Читает агрегированную статистику рецептов для внешних модулей.
/// </summary>
internal sealed class RecipeUserStatisticsReader(RecipesDbContext dbContext) : IUserRecipeStatisticsReader
{
    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<Guid, int>> GetActiveRecipeCountsByOwnerAsync(
        IReadOnlyCollection<Guid> userIds,
        CancellationToken cancellationToken)
    {
        UserId[] distinctUserIds =
        [
            .. userIds
            .Where(userId => userId != Guid.Empty)
            .Distinct()
            .Select(UserId.From),
        ];

        if (distinctUserIds.Length == 0)
        {
            return new Dictionary<Guid, int>();
        }

        return await dbContext.Recipes
            .AsNoTracking()
            .Where(recipe => !recipe.IsDeleted && distinctUserIds.Contains(recipe.OwnerUserId))
            .GroupBy(recipe => recipe.OwnerUserId)
            .Select(group => new { UserId = group.Key.Value, Count = group.Count() })
            .ToDictionaryAsync(item => item.UserId, item => item.Count, cancellationToken);
    }
}
