using MenuMate.Modules.Tags.Domain.Models;

namespace MenuMate.Modules.Tags.Application.Abstractions;

internal interface ITagsRepository
{
    Task<Tag?> GetByIdAsync(Guid tagId, CancellationToken cancellationToken);

    Task<bool> ExistsByNormalizedNameAsync(string normalizedName, CancellationToken cancellationToken);

    Task AddAsync(Tag tag, CancellationToken cancellationToken);

    Task UpdateAsync(Tag tag, CancellationToken cancellationToken);
}
