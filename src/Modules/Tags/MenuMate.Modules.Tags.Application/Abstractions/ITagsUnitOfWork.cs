namespace MenuMate.Modules.Tags.Application.Abstractions;

internal interface ITagsUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
