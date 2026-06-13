using MenuMate.Common.Application;
using MenuMate.Contracts.MenuPlanning;
using MenuMate.Modules.MenuPlanning.Application.Abstractions;
using MenuMate.Modules.MenuPlanning.Domain.ValueObjects;
using MenuMate.SharedKernel;
using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.MenuPlanning.Application;

internal sealed class GetMenuCalendarQueryHandler(
    IMenuCalendarReadDbContext readDbContext,
    IRecipeCoverImageReader recipeCoverImageReader,
    IUserContext userContext)
    : IQueryHandler<GetMenuCalendarQuery, MenuCalendarResponse>
{
    public async Task<Result<MenuCalendarResponse>> Handle(
        GetMenuCalendarQuery query,
        CancellationToken cancellationToken)
    {
        Result<MenuCalendarDateRange> dateRange = MenuCalendarDateRange.Create(query.StartDate, query.EndDate);
        if (dateRange.IsFailure)
        {
            return Result.Failure<MenuCalendarResponse>(dateRange.Error);
        }

        MenuCalendarResponse calendar =
            await readDbContext.GetCalendarAsync(userContext.UserId, dateRange.Value, cancellationToken);
        RecipeId[] recipeIds =
        [
            .. calendar.Items
                .Where(item => item.RecipeId.HasValue)
                .Select(item => RecipeId.From(item.RecipeId!.Value))
                .Distinct()
        ];
        IReadOnlyDictionary<Guid, Uri> imageUrls =
            await recipeCoverImageReader.GetReadUrlsAsync(recipeIds, cancellationToken);

        return calendar with
        {
            Items = calendar.Items
                .Select(item => item.RecipeId.HasValue && imageUrls.TryGetValue(item.RecipeId.Value, out Uri? imageUrl)
                    ? item with { ImageUrl = imageUrl }
                    : item)
                .ToArray()
        };
    }
}

internal sealed class GetMealSlotsQueryHandler(
    IMenuCalendarReadDbContext readDbContext,
    IUserContext userContext)
    : IQueryHandler<GetMealSlotsQuery, IReadOnlyCollection<MealSlotResponse>>
{
    public async Task<Result<IReadOnlyCollection<MealSlotResponse>>> Handle(
        GetMealSlotsQuery query,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<MealSlotResponse> mealSlots =
            await readDbContext.GetMealSlotsAsync(userContext.UserId, cancellationToken);

        return Result.Success(mealSlots);
    }
}
