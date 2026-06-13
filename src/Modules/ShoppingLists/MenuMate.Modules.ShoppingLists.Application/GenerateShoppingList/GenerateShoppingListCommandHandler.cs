using MenuMate.Common.Application;
using MenuMate.Contracts.ShoppingLists;
using MenuMate.Modules.ShoppingLists.Application.Abstractions;
using MenuMate.Modules.ShoppingLists.Domain.Models;
using MenuMate.Modules.ShoppingLists.Domain.Services;
using MenuMate.SharedKernel;
using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.ShoppingLists.Application.GenerateShoppingList;

internal sealed class GenerateShoppingListCommandHandler(
    IShoppingListSourceReader sourceReader,
    IShoppingListsRepository repository,
    IShoppingListsReadDbContext readDbContext,
    IShoppingListsUnitOfWork unitOfWork,
    IUserContext userContext,
    TimeProvider timeProvider)
    : ICommandHandler<GenerateShoppingListCommand, ShoppingListResponse>
{
    public async Task<Result<ShoppingListResponse>> Handle(
        GenerateShoppingListCommand command,
        CancellationToken cancellationToken)
    {
        if (command.Request.EndDate < command.Request.StartDate)
        {
            return Result.Failure<ShoppingListResponse>(ShoppingListApplicationErrors.InvalidDateRange);
        }

        if (command.Request.Recipes.Any(selection => selection.Servings <= 0))
        {
            return Result.Failure<ShoppingListResponse>(ShoppingListApplicationErrors.InvalidServings);
        }

        IReadOnlyCollection<ShoppingRecipe> recipes = await sourceReader.GetMenuCalendarRecipesAsync(
            userContext.UserId,
            command.Request.StartDate,
            command.Request.EndDate,
            cancellationToken);

        var selections = command.Request.Recipes
            .ToDictionary(selection => selection.MenuItemId);
        ShoppingRecipe[] selectedRecipes =
        [
            .. recipes
                .Where(recipe => selections.ContainsKey(recipe.MenuItemId))
                .Select(recipe =>
                {
                    MenuShoppingSelectionRequest selection = selections[recipe.MenuItemId];
                    var ingredientIds = selection.IngredientIds.ToHashSet();
                    return recipe with
                    {
                        TargetServings = selection.Servings,
                        Ingredients =
                        [
                            .. recipe.Ingredients.Where(ingredient =>
                                ingredientIds.Contains(
                                    ingredient.LineId == Guid.Empty ? ingredient.ProductId : ingredient.LineId))
                        ]
                    };
                })
        ];

        ShoppingList generatedList = ShoppingListGenerator.Generate(selectedRecipes);
        DateTimeOffset now = timeProvider.GetUtcNow();
        SavedShoppingList? existing = await repository.GetByOwnerAsync(userContext.UserId, cancellationToken);
        SavedShoppingList shoppingList;
        if (existing is null)
        {
            Result<SavedShoppingList> created = SavedShoppingList.Create(
                Guid.CreateVersion7(),
                userContext.UserId,
                command.Request.StartDate,
                command.Request.EndDate,
                generatedList.Categories.SelectMany(category => category.Items),
                now);
            if (created.IsFailure)
            {
                return Result.Failure<ShoppingListResponse>(created.Error);
            }

            shoppingList = created.Value;
            await repository.AddAsync(shoppingList, cancellationToken);
        }
        else
        {
            shoppingList = existing;
            Result replaced = shoppingList.Replace(
                command.Request.StartDate,
                command.Request.EndDate,
                generatedList.Categories.SelectMany(category => category.Items),
                now);
            if (replaced.IsFailure)
            {
                return Result.Failure<ShoppingListResponse>(replaced.Error);
            }

            await repository.UpdateAsync(shoppingList, cancellationToken);
        }
        await unitOfWork.SaveChangesAsync(cancellationToken);

        ShoppingListResponse? response = await readDbContext.GetCurrentShoppingListAsync(userContext.UserId, cancellationToken);

        return response is null
            ? Result.Failure<ShoppingListResponse>(ShoppingListApplicationErrors.NotFound(shoppingList.Id))
            : response;
    }
}
