using MenuMate.Modules.Tags.Application.Abstractions;
using MenuMate.Modules.Tags.Domain.Models;
using MenuMate.Modules.Tags.Infrastructure.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace MenuMate.Modules.Tags.Infrastructure.Database;

internal sealed class EfTagsRepository(TagsDbContext dbContext) : ITagsRepository
{
    public async Task<Tag?> GetByIdAsync(Guid tagId, CancellationToken cancellationToken)
    {
        TagRecord? record = await dbContext.Tags
            .AsNoTracking()
            .SingleOrDefaultAsync(tag => tag.Id == tagId, cancellationToken);

        return record?.ToDomain();
    }

    public Task<bool> ExistsByNormalizedNameAsync(string normalizedName, CancellationToken cancellationToken) =>
        dbContext.Tags.AnyAsync(tag => tag.NormalizedName == normalizedName, cancellationToken);

    public async Task AddAsync(Tag tag, CancellationToken cancellationToken)
    {
        await dbContext.Tags.AddAsync(TagRecord.FromDomain(tag), cancellationToken);
    }

    public async Task UpdateAsync(Tag tag, CancellationToken cancellationToken)
    {
        TagRecord? record = await dbContext.Tags
            .SingleOrDefaultAsync(existing => existing.Id == tag.Id, cancellationToken);

        if (record is null)
        {
            await AddAsync(tag, cancellationToken);
            return;
        }

        record.Apply(tag);
    }
}
