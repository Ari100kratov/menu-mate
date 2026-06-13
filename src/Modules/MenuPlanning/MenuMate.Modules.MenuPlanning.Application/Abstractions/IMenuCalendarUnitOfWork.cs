namespace MenuMate.Modules.MenuPlanning.Application.Abstractions;

internal interface IMenuCalendarUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
