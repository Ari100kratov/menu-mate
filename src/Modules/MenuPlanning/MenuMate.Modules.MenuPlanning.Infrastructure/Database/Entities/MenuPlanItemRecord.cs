using MenuMate.Modules.MenuPlanning.Domain.Enums;
using MenuMate.Modules.MenuPlanning.Domain.Models;
using MenuMate.Modules.MenuPlanning.Domain.ValueObjects;
using MenuMate.SharedKernel;
using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.MenuPlanning.Infrastructure.Database.Entities;

internal sealed class MenuPlanItemRecord
{
    public Guid Id { get; set; }

    public Guid MenuPlanId { get; set; }

    public DateOnly Date { get; set; }

    public MealType MealType { get; set; }

    public int Servings { get; set; }

    public RecipeId? RecipeId { get; set; }

    public string? RecipeTitle { get; set; }

    public string? Text { get; set; }

    public string? Comment { get; set; }

    public static MenuPlanItemRecord FromDomain(MenuPlanItem item) =>
        new()
        {
            Id = item.Id,
            Date = item.Date,
            MealType = item.MealType,
            Servings = item.Servings.Value,
            RecipeId = item.RecipeId,
            RecipeTitle = item.RecipeTitle,
            Text = item.Text,
            Comment = item.Comment
        };

    public MenuPlanItem ToDomain()
    {
        Result<MenuServings> servings = MenuServings.Create(Servings);
        if (servings.IsFailure)
        {
            throw new DomainException(servings.Error);
        }

        return MenuPlanItem.Rehydrate(
            Id,
            Date,
            MealType,
            servings.Value,
            RecipeId,
            RecipeTitle,
            Text,
            Comment);
    }
}
