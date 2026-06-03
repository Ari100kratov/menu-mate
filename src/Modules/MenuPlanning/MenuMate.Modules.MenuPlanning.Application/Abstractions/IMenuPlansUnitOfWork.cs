namespace MenuMate.Modules.MenuPlanning.Application.Abstractions;

internal interface IMenuPlansUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
