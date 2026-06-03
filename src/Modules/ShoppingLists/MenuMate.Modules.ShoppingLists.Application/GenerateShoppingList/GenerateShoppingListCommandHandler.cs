using MenuMate.Common.Application;
using MenuMate.Contracts.ShoppingLists;
using MenuMate.Modules.ShoppingLists.Application.Abstractions;
using MenuMate.Modules.ShoppingLists.Domain.Models;
using MenuMate.Modules.ShoppingLists.Domain.Services;
using MenuMate.Modules.ShoppingLists.Domain.ValueObjects;
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
        var menuPlanId = MenuPlanId.From(command.Request.MenuPlanId);
        IReadOnlyCollection<ShoppingRecipe>? recipes = await sourceReader.GetMenuPlanRecipesAsync(
            menuPlanId,
            userContext.UserId,
            cancellationToken);

        if (recipes is null)
        {
            return Result.Failure<ShoppingListResponse>(
                ShoppingListApplicationErrors.MenuPlanNotFound(command.Request.MenuPlanId));
        }

        Result<IReadOnlyCollection<ManualShoppingListLine>> manualLines = MapManualLines(command.Request.ManualItems);
        if (manualLines.IsFailure)
        {
            return Result.Failure<ShoppingListResponse>(manualLines.Error);
        }

        ShoppingList generatedList = ShoppingListGenerator.Generate(recipes, manualLines.Value);
        DateTimeOffset now = timeProvider.GetUtcNow();
        Result<SavedShoppingList> shoppingList = SavedShoppingList.Create(
            Guid.CreateVersion7(),
            userContext.UserId,
            menuPlanId,
            generatedList.Categories.SelectMany(category => category.Items),
            now);

        if (shoppingList.IsFailure)
        {
            return Result.Failure<ShoppingListResponse>(shoppingList.Error);
        }

        await repository.AddAsync(shoppingList.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        ShoppingListResponse? response = await readDbContext.GetShoppingListAsync(
            shoppingList.Value.Id,
            userContext.UserId,
            cancellationToken);

        return response is null
            ? Result.Failure<ShoppingListResponse>(ShoppingListApplicationErrors.NotFound(shoppingList.Value.Id))
            : response;
    }

    private static Result<IReadOnlyCollection<ManualShoppingListLine>> MapManualLines(
        IReadOnlyCollection<ShoppingListItemRequest> requests)
    {
        var result = new List<ManualShoppingListLine>(requests.Count);

        foreach (ShoppingListItemRequest request in requests)
        {
            Result<SavedShoppingListItem> item = ShoppingListItemRequestMapper.Map(Guid.CreateVersion7(), request);
            if (item.IsFailure)
            {
                return Result.Failure<IReadOnlyCollection<ManualShoppingListLine>>(item.Error);
            }

            result.Add(new ManualShoppingListLine(
                item.Value.Name,
                item.Value.NormalizedName,
                item.Value.Amount,
                item.Value.Unit,
                item.Value.QuantityKind,
                item.Value.Category,
                item.Value.Comment));
        }

        return result;
    }
}
