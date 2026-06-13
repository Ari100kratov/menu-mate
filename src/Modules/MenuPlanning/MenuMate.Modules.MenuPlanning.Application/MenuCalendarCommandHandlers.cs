using MenuMate.Common.Application;
using MenuMate.Contracts.MenuPlanning;
using MenuMate.Modules.MenuPlanning.Application.Abstractions;
using MenuMate.Modules.MenuPlanning.Domain.Errors;
using MenuMate.Modules.MenuPlanning.Domain.Models;
using MenuMate.Modules.MenuPlanning.Domain.ValueObjects;
using MenuMate.SharedKernel;
using MenuMate.SharedKernel.Identifiers;
using static MenuMate.Modules.MenuPlanning.Application.MenuCalendarCommandHandlerHelpers;

namespace MenuMate.Modules.MenuPlanning.Application;

internal sealed class AddMenuCalendarItemCommandHandler(
    IMenuCalendarRepository repository,
    IRecipeRevisionAccessReader recipeRevisionAccessReader,
    IMenuCalendarUnitOfWork unitOfWork,
    IUserContext userContext,
    TimeProvider timeProvider)
    : ICommandHandler<AddMenuCalendarItemCommand, MenuCalendarItemResponse>
{
    public async Task<Result<MenuCalendarItemResponse>> Handle(
        AddMenuCalendarItemCommand command,
        CancellationToken cancellationToken)
    {
        Result<MealSlot> mealSlot = await GetOwnedMealSlotAsync(
            repository,
            userContext.UserId,
            command.Request.MealSlotId,
            cancellationToken);
        if (mealSlot.IsFailure)
        {
            return Result.Failure<MenuCalendarItemResponse>(mealSlot.Error);
        }

        int position = await repository.GetNextItemPositionAsync(
            userContext.UserId,
            command.Request.Date,
            command.Request.MealSlotId,
            cancellationToken);
        DateTimeOffset now = timeProvider.GetUtcNow();
        Result<MenuCalendarItem> item = MenuCalendarItemRequestMapper.Map(
            Guid.CreateVersion7(),
            userContext.UserId,
            position,
            command.Request,
            now);
        if (item.IsFailure)
        {
            return Result.Failure<MenuCalendarItemResponse>(item.Error);
        }

        if (!await CanUseRecipeAsync(item.Value, recipeRevisionAccessReader, userContext.UserId, cancellationToken))
        {
            return Result.Failure<MenuCalendarItemResponse>(MenuPlanningApplicationErrors.AccessDenied);
        }

        await repository.AddItemAsync(item.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return MenuCalendarMapping.ToResponse(item.Value);
    }
}

internal sealed class UpdateMenuCalendarItemCommandHandler(
    IMenuCalendarRepository repository,
    IRecipeRevisionAccessReader recipeRevisionAccessReader,
    IMenuCalendarUnitOfWork unitOfWork,
    IUserContext userContext,
    TimeProvider timeProvider)
    : ICommandHandler<UpdateMenuCalendarItemCommand, MenuCalendarItemResponse>
{
    public async Task<Result<MenuCalendarItemResponse>> Handle(
        UpdateMenuCalendarItemCommand command,
        CancellationToken cancellationToken)
    {
        MenuCalendarItem? item = await repository.GetItemByIdAsync(command.ItemId, cancellationToken);
        if (item is null)
        {
            return Result.Failure<MenuCalendarItemResponse>(
                MenuPlanningApplicationErrors.ItemNotFound(command.ItemId));
        }

        if (item.OwnerUserId != userContext.UserId)
        {
            return Result.Failure<MenuCalendarItemResponse>(MenuPlanningApplicationErrors.AccessDenied);
        }

        Result<MealSlot> mealSlot = await GetOwnedMealSlotAsync(
            repository,
            userContext.UserId,
            command.Request.MealSlotId,
            cancellationToken);
        if (mealSlot.IsFailure)
        {
            return Result.Failure<MenuCalendarItemResponse>(mealSlot.Error);
        }

        Result<MenuServings> servings = MenuServings.Create(command.Request.Servings);
        if (servings.IsFailure)
        {
            return Result.Failure<MenuCalendarItemResponse>(servings.Error);
        }

        RecipeId? recipeId = command.Request.RecipeId.HasValue
            ? RecipeId.From(command.Request.RecipeId.Value)
            : null;
        RecipeRevisionId? revisionId = command.Request.RecipeRevisionId.HasValue
            ? RecipeRevisionId.From(command.Request.RecipeRevisionId.Value)
            : null;
        bool placementChanged = item.Date != command.Request.Date || item.MealSlotId != command.Request.MealSlotId;
        DateTimeOffset now = timeProvider.GetUtcNow();

        Result update = item.Update(
            command.Request.Date,
            command.Request.MealSlotId,
            recipeId,
            revisionId,
            command.Request.RecipeTitle,
            command.Request.Text,
            servings.Value,
            now,
            command.Request.Comment);
        if (update.IsFailure)
        {
            return Result.Failure<MenuCalendarItemResponse>(update.Error);
        }

        if (!await CanUseRecipeAsync(item, recipeRevisionAccessReader, userContext.UserId, cancellationToken))
        {
            return Result.Failure<MenuCalendarItemResponse>(MenuPlanningApplicationErrors.AccessDenied);
        }

        if (placementChanged)
        {
            int position = await repository.GetNextItemPositionAsync(
                userContext.UserId,
                item.Date,
                item.MealSlotId,
                cancellationToken);
            Result positionResult = item.ChangePosition(position, now);
            if (positionResult.IsFailure)
            {
                return Result.Failure<MenuCalendarItemResponse>(positionResult.Error);
            }
        }

        await repository.UpdateItemAsync(item, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return MenuCalendarMapping.ToResponse(item);
    }
}

internal sealed class RemoveMenuCalendarItemCommandHandler(
    IMenuCalendarRepository repository,
    IMenuCalendarUnitOfWork unitOfWork,
    IUserContext userContext)
    : ICommandHandler<RemoveMenuCalendarItemCommand>
{
    public async Task<Result> Handle(RemoveMenuCalendarItemCommand command, CancellationToken cancellationToken)
    {
        MenuCalendarItem? item = await repository.GetItemByIdAsync(command.ItemId, cancellationToken);
        if (item is null)
        {
            return Result.Failure(MenuPlanningApplicationErrors.ItemNotFound(command.ItemId));
        }

        if (item.OwnerUserId != userContext.UserId)
        {
            return Result.Failure(MenuPlanningApplicationErrors.AccessDenied);
        }

        await repository.DeleteItemAsync(item, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

internal sealed class ClearMenuCalendarCommandHandler(
    IMenuCalendarRepository repository,
    IMenuCalendarUnitOfWork unitOfWork,
    IUserContext userContext)
    : ICommandHandler<ClearMenuCalendarCommand>
{
    public async Task<Result> Handle(ClearMenuCalendarCommand command, CancellationToken cancellationToken)
    {
        Result<MenuCalendarDateRange> dateRange = MenuCalendarDateRange.Create(command.StartDate, command.EndDate);
        if (dateRange.IsFailure)
        {
            return Result.Failure(dateRange.Error);
        }

        await repository.ClearItemsAsync(userContext.UserId, dateRange.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

internal sealed class CreateMealSlotCommandHandler(
    IMenuCalendarRepository repository,
    IMenuCalendarReadDbContext readDbContext,
    IMenuCalendarUnitOfWork unitOfWork,
    IUserContext userContext,
    TimeProvider timeProvider)
    : ICommandHandler<CreateMealSlotCommand, IReadOnlyCollection<MealSlotResponse>>
{
    public async Task<Result<IReadOnlyCollection<MealSlotResponse>>> Handle(
        CreateMealSlotCommand command,
        CancellationToken cancellationToken)
    {
        if (await repository.MealSlotNameExistsAsync(
                userContext.UserId,
                command.Request.Name,
                null,
                cancellationToken))
        {
            return Result.Failure<IReadOnlyCollection<MealSlotResponse>>(
                MenuPlanningApplicationErrors.DuplicateMealSlotName);
        }

        int sortOrder = await repository.GetNextMealSlotSortOrderAsync(userContext.UserId, cancellationToken);
        Result<MealSlot> mealSlot = MealSlot.Create(
            Guid.CreateVersion7(),
            userContext.UserId,
            command.Request.Name,
            sortOrder,
            timeProvider.GetUtcNow());
        if (mealSlot.IsFailure)
        {
            return Result.Failure<IReadOnlyCollection<MealSlotResponse>>(mealSlot.Error);
        }

        await repository.AddMealSlotAsync(mealSlot.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(await readDbContext.GetMealSlotsAsync(userContext.UserId, cancellationToken));
    }
}

internal sealed class UpdateMealSlotCommandHandler(
    IMenuCalendarRepository repository,
    IMenuCalendarReadDbContext readDbContext,
    IMenuCalendarUnitOfWork unitOfWork,
    IUserContext userContext,
    TimeProvider timeProvider)
    : ICommandHandler<UpdateMealSlotCommand, IReadOnlyCollection<MealSlotResponse>>
{
    public async Task<Result<IReadOnlyCollection<MealSlotResponse>>> Handle(
        UpdateMealSlotCommand command,
        CancellationToken cancellationToken)
    {
        Result<MealSlot> mealSlotResult = await GetOwnedMealSlotAsync(
            repository,
            userContext.UserId,
            command.MealSlotId,
            cancellationToken);
        if (mealSlotResult.IsFailure)
        {
            return Result.Failure<IReadOnlyCollection<MealSlotResponse>>(mealSlotResult.Error);
        }

        if (await repository.MealSlotNameExistsAsync(
                userContext.UserId,
                command.Request.Name,
                command.MealSlotId,
                cancellationToken))
        {
            return Result.Failure<IReadOnlyCollection<MealSlotResponse>>(
                MenuPlanningApplicationErrors.DuplicateMealSlotName);
        }

        MealSlot mealSlot = mealSlotResult.Value;
        Result rename = mealSlot.Rename(command.Request.Name, timeProvider.GetUtcNow());
        if (rename.IsFailure)
        {
            return Result.Failure<IReadOnlyCollection<MealSlotResponse>>(rename.Error);
        }

        await repository.UpdateMealSlotAsync(mealSlot, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(await readDbContext.GetMealSlotsAsync(userContext.UserId, cancellationToken));
    }
}

internal sealed class DeleteMealSlotCommandHandler(
    IMenuCalendarRepository repository,
    IMenuCalendarReadDbContext readDbContext,
    IMenuCalendarUnitOfWork unitOfWork,
    IUserContext userContext,
    TimeProvider timeProvider)
    : ICommandHandler<DeleteMealSlotCommand, IReadOnlyCollection<MealSlotResponse>>
{
    public async Task<Result<IReadOnlyCollection<MealSlotResponse>>> Handle(
        DeleteMealSlotCommand command,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<MealSlot> mealSlots = await repository.GetMealSlotsAsync(
            userContext.UserId,
            cancellationToken);
        MealSlot? mealSlot = mealSlots.SingleOrDefault(slot => slot.Id == command.MealSlotId);
        if (mealSlot is null)
        {
            return Result.Failure<IReadOnlyCollection<MealSlotResponse>>(
                MenuPlanningApplicationErrors.MealSlotNotFound(command.MealSlotId));
        }

        if (mealSlots.Count == 1)
        {
            return Result.Failure<IReadOnlyCollection<MealSlotResponse>>(MenuCalendarErrors.CannotDeleteLastMealSlot);
        }

        if (await repository.HasItemsForMealSlotAsync(userContext.UserId, mealSlot.Id, cancellationToken))
        {
            return Result.Failure<IReadOnlyCollection<MealSlotResponse>>(
                MenuPlanningApplicationErrors.MealSlotHasItems(mealSlot.Id));
        }

        DateTimeOffset now = timeProvider.GetUtcNow();
        await repository.DeleteMealSlotAsync(mealSlot, cancellationToken);

        int index = 0;
        foreach (MealSlot remaining in mealSlots.Where(slot => slot.Id != mealSlot.Id).OrderBy(slot => slot.SortOrder))
        {
            remaining.ChangeSortOrder(index++, now);
            await repository.UpdateMealSlotAsync(remaining, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(await readDbContext.GetMealSlotsAsync(userContext.UserId, cancellationToken));
    }
}

internal sealed class ReorderMealSlotsCommandHandler(
    IMenuCalendarRepository repository,
    IMenuCalendarReadDbContext readDbContext,
    IMenuCalendarUnitOfWork unitOfWork,
    IUserContext userContext,
    TimeProvider timeProvider)
    : ICommandHandler<ReorderMealSlotsCommand, IReadOnlyCollection<MealSlotResponse>>
{
    public async Task<Result<IReadOnlyCollection<MealSlotResponse>>> Handle(
        ReorderMealSlotsCommand command,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<MealSlot> mealSlots = await repository.GetMealSlotsAsync(
            userContext.UserId,
            cancellationToken);
        Guid[] requestedIds = [.. command.Request.MealSlotIds];
        if (requestedIds.Length != mealSlots.Count ||
            requestedIds.Distinct().Count() != requestedIds.Length ||
            requestedIds.Any(id => mealSlots.All(slot => slot.Id != id)))
        {
            return Result.Failure<IReadOnlyCollection<MealSlotResponse>>(
                MenuPlanningApplicationErrors.InvalidMealSlotOrder);
        }

        DateTimeOffset now = timeProvider.GetUtcNow();
        for (int index = 0; index < requestedIds.Length; index++)
        {
            MealSlot mealSlot = mealSlots.Single(slot => slot.Id == requestedIds[index]);
            mealSlot.ChangeSortOrder(index, now);
            await repository.UpdateMealSlotAsync(mealSlot, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(await readDbContext.GetMealSlotsAsync(userContext.UserId, cancellationToken));
    }
}

internal static class MenuCalendarCommandHandlerHelpers
{
    public static async Task<Result<MealSlot>> GetOwnedMealSlotAsync(
        IMenuCalendarRepository repository,
        UserId ownerUserId,
        Guid mealSlotId,
        CancellationToken cancellationToken)
    {
        MealSlot? mealSlot = await repository.GetMealSlotByIdAsync(
            ownerUserId,
            mealSlotId,
            cancellationToken);
        if (mealSlot is null)
        {
            return Result.Failure<MealSlot>(MenuPlanningApplicationErrors.MealSlotNotFound(mealSlotId));
        }

        return mealSlot.OwnerUserId == ownerUserId
            ? mealSlot
            : Result.Failure<MealSlot>(MenuPlanningApplicationErrors.AccessDenied);
    }

    public static Task<bool> CanUseRecipeAsync(
        MenuCalendarItem item,
        IRecipeRevisionAccessReader recipeRevisionAccessReader,
        UserId userId,
        CancellationToken cancellationToken) =>
        !item.IsRecipeItem
            ? Task.FromResult(true)
            : recipeRevisionAccessReader.CanUseAsync(
                userId,
                item.RecipeId!.Value,
                item.RecipeRevisionId!.Value,
                cancellationToken);
}
