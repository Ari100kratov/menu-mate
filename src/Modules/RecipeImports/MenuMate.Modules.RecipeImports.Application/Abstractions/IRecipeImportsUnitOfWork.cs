namespace MenuMate.Modules.RecipeImports.Application.Abstractions;

internal interface IRecipeImportsUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
