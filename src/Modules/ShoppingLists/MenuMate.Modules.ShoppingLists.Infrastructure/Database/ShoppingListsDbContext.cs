using MenuMate.Contracts.ShoppingLists;
using MenuMate.Modules.ShoppingLists.Application.Abstractions;
using MenuMate.Modules.ShoppingLists.Domain.Models;
using MenuMate.Modules.ShoppingLists.Domain.Services;
using MenuMate.Modules.ShoppingLists.Domain.ValueObjects;
using MenuMate.Modules.ShoppingLists.Infrastructure.Database.Entities;
using MenuMate.Modules.ShoppingLists.Infrastructure.Database.Source;
using MenuMate.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace MenuMate.Modules.ShoppingLists.Infrastructure.Database;

/// <summary>
/// EF Core DbContext модуля ShoppingLists.
/// </summary>
public sealed class ShoppingListsDbContext(DbContextOptions<ShoppingListsDbContext> options)
    : DbContext(options), IShoppingListsUnitOfWork, IShoppingListsReadDbContext, IShoppingListSourceReader
{
    internal DbSet<ShoppingListRecord> ShoppingLists => Set<ShoppingListRecord>();

    private DbSet<MenuCalendarItemSourceRecord> MenuCalendarItems => Set<MenuCalendarItemSourceRecord>();

    private DbSet<RecipeRevisionSourceRecord> RecipeRevisions => Set<RecipeRevisionSourceRecord>();

    /// <inheritdoc />
    public async Task<ShoppingListResponse?> GetShoppingListAsync(
        Guid shoppingListId,
        UserId ownerUserId,
        CancellationToken cancellationToken)
    {
        ShoppingListRecord? record = await ShoppingLists
            .AsNoTracking()
            .Include(shoppingList => shoppingList.Items)
            .SingleOrDefaultAsync(
                shoppingList => shoppingList.Id == shoppingListId && shoppingList.OwnerUserId == ownerUserId,
                cancellationToken);

        return record is null ? null : ToResponse(record.ToDomain());
    }

    /// <inheritdoc />
    public async Task<ShoppingListResponse?> GetCurrentShoppingListAsync(
        UserId ownerUserId,
        CancellationToken cancellationToken)
    {
        ShoppingListRecord? record = await ShoppingLists
            .AsNoTracking()
            .Include(shoppingList => shoppingList.Items)
            .SingleOrDefaultAsync(shoppingList => shoppingList.OwnerUserId == ownerUserId, cancellationToken);

        return record is null ? null : ToResponse(record.ToDomain());
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<ShoppingRecipe>> GetMenuCalendarRecipesAsync(
        UserId ownerUserId,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken)
    {
        MenuCalendarItemSourceRecord[] items = await MenuCalendarItems
            .AsNoTracking()
            .Where(item =>
                item.OwnerUserId == ownerUserId &&
                item.Date >= startDate &&
                item.Date <= endDate)
            .ToArrayAsync(cancellationToken);

        RecipeRevisionId[] revisionIds =
        [
            .. items
                .Where(item => item.RecipeRevisionId.HasValue)
                .Select(item => item.RecipeRevisionId!.Value)
                .Distinct()
        ];

        Dictionary<RecipeRevisionId, RecipeRevisionSourceRecord> revisions = await RecipeRevisions
            .AsNoTracking()
            .Include(revision => revision.Ingredients)
            .Where(revision => revisionIds.Contains(revision.Id))
            .ToDictionaryAsync(revision => revision.Id, cancellationToken);

        return
        [
            .. items
                .Where(item => item.RecipeRevisionId.HasValue && revisions.ContainsKey(item.RecipeRevisionId.Value))
                .Select(item => CreateShoppingRecipe(
                    item.Id,
                    item.RecipeTitle ?? "Рецепт",
                    revisions[item.RecipeRevisionId!.Value],
                    item.Servings))
        ];
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.HasDefaultSchema(ShoppingListsSchema.Name);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ShoppingListsDbContext).Assembly);
    }

    private static ShoppingRecipe CreateShoppingRecipe(
        Guid menuItemId,
        string title,
        RecipeRevisionSourceRecord revision,
        int targetServings) =>
        new(
            revision.RecipeId,
            revision.Servings,
            targetServings,
            [
                .. revision.Ingredients
                    .OrderBy(ingredient => ingredient.Order)
                    .Select(ingredient => new ShoppingIngredientLine(
                        ingredient.IngredientId,
                        ingredient.ProductName,
                        ingredient.NormalizedProductName,
                        ingredient.Amount,
                        ingredient.Unit,
                        ingredient.Category,
                        ingredient.Comment,
                        ingredient.IsOptional,
                        ingredient.Id))
            ],
            menuItemId,
            title);

    private static ShoppingListResponse ToResponse(SavedShoppingList shoppingList)
    {
        ShoppingList grouped = shoppingList.ToGroupedShoppingList();
        return new ShoppingListResponse(
            shoppingList.Id,
            shoppingList.SourceStartDate,
            shoppingList.SourceEndDate,
            shoppingList.CreatedAt,
            shoppingList.UpdatedAt,
            [
                .. grouped.Categories.Select(category => new ShoppingListCategoryResponse(
                    category.Name,
                    [
                        .. shoppingList.Items
                            .Where(item => item.Category == category.Category)
                            .OrderBy(item => item.Name)
                            .Select(ToResponse)
                    ]))
            ],
            ShoppingListTextFormatter.Format(grouped));
    }

    private static ShoppingListItemResponse ToResponse(SavedShoppingListItem item) =>
        new(
            item.Id,
            item.ProductId,
            item.Name,
            item.Amount,
            item.Unit.ToString(),
            item.Category.ToString(),
            ShoppingListTextFormatter.FormatAmount(item.ToShoppingListItem()),
            item.Comment,
            item.IsPurchased);
}
