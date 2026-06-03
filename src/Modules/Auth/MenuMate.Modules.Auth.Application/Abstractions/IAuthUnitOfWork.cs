namespace MenuMate.Modules.Auth.Application.Abstractions;

internal interface IAuthUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
