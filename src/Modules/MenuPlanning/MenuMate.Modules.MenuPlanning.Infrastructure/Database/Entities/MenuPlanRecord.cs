using MenuMate.Modules.MenuPlanning.Domain.Models;
using MenuMate.Modules.MenuPlanning.Domain.ValueObjects;
using MenuMate.SharedKernel;
using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.MenuPlanning.Infrastructure.Database.Entities;

internal sealed class MenuPlanRecord
{
    public Guid Id { get; set; }

    public UserId OwnerUserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public List<MenuPlanItemRecord> Items { get; set; } = [];

    public static MenuPlanRecord FromDomain(MenuPlan menuPlan)
    {
        var record = new MenuPlanRecord();
        record.Apply(menuPlan);
        return record;
    }

    public void Apply(MenuPlan menuPlan)
    {
        Id = menuPlan.Id;
        OwnerUserId = menuPlan.OwnerUserId;
        Name = menuPlan.Name;
        StartDate = menuPlan.DateRange.StartDate;
        EndDate = menuPlan.DateRange.EndDate;
        CreatedAt = menuPlan.CreatedAt;
        UpdatedAt = menuPlan.UpdatedAt;

        Items.Clear();
        Items.AddRange(menuPlan.Items.Select(MenuPlanItemRecord.FromDomain));

        foreach (MenuPlanItemRecord item in Items)
        {
            item.MenuPlanId = Id;
        }
    }

    public MenuPlan ToDomain()
    {
        Result<MenuPlanDateRange> dateRange = MenuPlanDateRange.Create(StartDate, EndDate);
        if (dateRange.IsFailure)
        {
            throw new DomainException(dateRange.Error);
        }

        return MenuPlan.Rehydrate(
            Id,
            OwnerUserId,
            Name,
            dateRange.Value,
            CreatedAt,
            UpdatedAt,
            Items.Select(item => item.ToDomain()));
    }
}
