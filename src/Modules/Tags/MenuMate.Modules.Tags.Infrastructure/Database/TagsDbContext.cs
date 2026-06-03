using MenuMate.Contracts.Tags;
using MenuMate.Modules.Tags.Application.Abstractions;
using MenuMate.Modules.Tags.Domain.Enums;
using MenuMate.Modules.Tags.Infrastructure.Database.Entities;
using MenuMate.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace MenuMate.Modules.Tags.Infrastructure.Database;

/// <summary>
/// EF Core DbContext модуля Tags.
/// </summary>
public sealed class TagsDbContext(DbContextOptions<TagsDbContext> options)
    : DbContext(options), ITagsUnitOfWork, ITagsReadDbContext
{
    internal DbSet<TagRecord> Tags => Set<TagRecord>();

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<TagResponse>> GetTagsAsync(
        string? search,
        bool includeHidden,
        CancellationToken cancellationToken)
    {
        IQueryable<TagRecord> query = Tags.AsNoTracking();

        if (!includeHidden)
        {
            query = query.Where(tag => tag.Status != TagStatus.Hidden);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            string normalized = TextNormalizer.NormalizeSearchText(search);
            query = query.Where(tag => tag.NormalizedName.Contains(normalized));
        }

        return await query
            .OrderBy(tag => tag.Name)
            .Select(tag => new TagResponse(
                tag.Id,
                tag.Name,
                tag.NormalizedName,
                tag.Kind.ToString(),
                tag.Status.ToString(),
                tag.CreatedAt,
                tag.UpdatedAt))
            .ToArrayAsync(cancellationToken);
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.HasDefaultSchema(TagsSchema.Name);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TagsDbContext).Assembly);
    }
}
