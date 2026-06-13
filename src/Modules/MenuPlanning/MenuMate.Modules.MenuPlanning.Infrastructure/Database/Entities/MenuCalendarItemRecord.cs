using MenuMate.Modules.MenuPlanning.Domain.Models;
using MenuMate.Modules.MenuPlanning.Domain.ValueObjects;
using MenuMate.SharedKernel;
using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.MenuPlanning.Infrastructure.Database.Entities;

internal sealed class MenuCalendarItemRecord
{
    public Guid Id { get; set; }

    public UserId OwnerUserId { get; set; }

    public DateOnly Date { get; set; }

    public Guid MealSlotId { get; set; }

    public int Position { get; set; }

    public int Servings { get; set; }

    public RecipeId? RecipeId { get; set; }

    public RecipeRevisionId? RecipeRevisionId { get; set; }

    public string? RecipeTitle { get; set; }

    public string? Text { get; set; }

    public string? Comment { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public static MenuCalendarItemRecord FromDomain(MenuCalendarItem item)
    {
        var record = new MenuCalendarItemRecord();
        record.Apply(item);
        return record;
    }

    public void Apply(MenuCalendarItem item)
    {
        Id = item.Id;
        OwnerUserId = item.OwnerUserId;
        Date = item.Date;
        MealSlotId = item.MealSlotId;
        Position = item.Position;
        Servings = item.Servings.Value;
        RecipeId = item.RecipeId;
        RecipeRevisionId = item.RecipeRevisionId;
        RecipeTitle = item.RecipeTitle;
        Text = item.Text;
        Comment = item.Comment;
        CreatedAt = item.CreatedAt;
        UpdatedAt = item.UpdatedAt;
    }

    public MenuCalendarItem ToDomain()
    {
        Result<MenuServings> servings = MenuServings.Create(Servings);
        if (servings.IsFailure)
        {
            throw new DomainException(servings.Error);
        }

        return MenuCalendarItem.Rehydrate(
            Id,
            OwnerUserId,
            Date,
            MealSlotId,
            Position,
            servings.Value,
            RecipeId,
            RecipeRevisionId,
            RecipeTitle,
            Text,
            Comment,
            CreatedAt,
            UpdatedAt);
    }
}
