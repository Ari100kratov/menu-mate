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

    private DbSet<MenuPlanSourceRecord> MenuPlans => Set<MenuPlanSourceRecord>();

    private DbSet<RecipeRevisionSourceRecord> RecipeRevisions => Set<RecipeRevisionSourceRecord>();

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<ShoppingListSummaryResponse>> GetShoppingListsAsync(
        UserId ownerUserId,
        CancellationToken cancellationToken)
    {
        return await ShoppingLists
            .AsNoTracking()
            .Where(shoppingList => shoppingList.OwnerUserId == ownerUserId)
            .OrderByDescending(shoppingList => shoppingList.CreatedAt)
            .Select(shoppingList => new ShoppingListSummaryResponse(
                shoppingList.Id,
                shoppingList.SourceMenuPlanId.Value,
                shoppingList.CreatedAt,
                shoppingList.UpdatedAt,
                shoppingList.Items.Count,
                shoppingList.Items.Count(item => item.IsPurchased)))
            .ToArrayAsync(cancellationToken);
    }

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
    public async Task<IReadOnlyCollection<ShoppingRecipe>?> GetMenuPlanRecipesAsync(
        MenuPlanId menuPlanId,
        UserId ownerUserId,
        CancellationToken cancellationToken)
    {
        MenuPlanSourceRecord? menuPlan = await MenuPlans
            .AsNoTracking()
            .Include(menuPlanSource => menuPlanSource.Items)
            .SingleOrDefaultAsync(
                menuPlanSource => menuPlanSource.Id == menuPlanId && menuPlanSource.OwnerUserId == ownerUserId,
                cancellationToken);

        if (menuPlan is null)
        {
            return null;
        }

        RecipeRevisionId[] revisionIds =
        [
            .. menuPlan.Items
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
            .. menuPlan.Items
                .Where(item => item.RecipeRevisionId.HasValue && revisions.ContainsKey(item.RecipeRevisionId.Value))
                .Select(item => CreateShoppingRecipe(revisions[item.RecipeRevisionId!.Value], item.Servings))
        ];
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.HasDefaultSchema(ShoppingListsSchema.Name);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ShoppingListsDbContext).Assembly);
    }

    private static ShoppingRecipe CreateShoppingRecipe(RecipeRevisionSourceRecord revision, int targetServings) =>
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
                        ingredient.QuantityKind,
                        ingredient.Category,
                        ingredient.Comment,
                        ingredient.IsOptional))
            ]);

    private static ShoppingListResponse ToResponse(SavedShoppingList shoppingList)
    {
        ShoppingList grouped = shoppingList.ToGroupedShoppingList();
        return new ShoppingListResponse(
            shoppingList.Id,
            shoppingList.SourceMenuPlanId.Value,
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
            item.QuantityKind.ToString(),
            item.Category.ToString(),
            ShoppingListTextFormatter.FormatAmount(item.ToShoppingListItem()),
            item.Comment,
            item.IsPurchased,
            item.IsInStock);
}
