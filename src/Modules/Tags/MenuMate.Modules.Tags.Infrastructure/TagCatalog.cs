using MenuMate.Common.Application.Tags;
using MenuMate.Modules.Tags.Domain.Enums;
using MenuMate.Modules.Tags.Domain.ValueObjects;
using MenuMate.Modules.Tags.Infrastructure.Database;
using MenuMate.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace MenuMate.Modules.Tags.Infrastructure;

/// <summary>
/// Persistence-реализация общего каталога тегов.
/// </summary>
public sealed class TagCatalog(TagsDbContext dbContext, TimeProvider timeProvider) : ITagCatalog
{
    /// <inheritdoc />
    public async Task<IReadOnlyCollection<TagCatalogItem>> ResolveAsync(
        IReadOnlyCollection<string> names,
        TagCatalogSource source,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(names);

        var uniqueNames = new Dictionary<string, TagName>(StringComparer.Ordinal);
        foreach (string nameValue in names)
        {
            Result<TagName> name = TagName.Create(nameValue);
            if (name.IsFailure)
            {
                throw new DomainException(name.Error);
            }

            uniqueNames.TryAdd(name.Value.NormalizedValue, name.Value);
        }

        if (uniqueNames.Count == 0)
        {
            return [];
        }

        string kind = source == TagCatalogSource.Suggested
            ? TagKind.Suggested.ToString()
            : TagKind.User.ToString();
        DateTimeOffset now = timeProvider.GetUtcNow();

        foreach (TagName name in uniqueNames.Values)
        {
            await dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"""
                INSERT INTO tags.tags
                    (id, name, normalized_name, kind, status, created_at, updated_at)
                VALUES
                    ({Guid.CreateVersion7()}, {name.Value}, {name.NormalizedValue},
                     {kind}, {TagStatus.Confirmed.ToString()}, {now}, {now})
                ON CONFLICT (normalized_name) DO NOTHING
                """,
                cancellationToken);
        }

        string[] normalizedNames = [.. uniqueNames.Keys];
        return await dbContext.Tags
            .AsNoTracking()
            .Where(tag => normalizedNames.Contains(tag.NormalizedName))
            .OrderBy(tag => tag.Name)
            .Select(tag => new TagCatalogItem(tag.Id, tag.Name))
            .ToArrayAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<Guid, string>> GetNamesAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(ids);
        if (ids.Count == 0)
        {
            return new Dictionary<Guid, string>();
        }

        Guid[] uniqueIds = [.. ids.Distinct()];
        return await dbContext.Tags
            .AsNoTracking()
            .Where(tag => uniqueIds.Contains(tag.Id))
            .ToDictionaryAsync(tag => tag.Id, tag => tag.Name, cancellationToken);
    }
}
