using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.ShoppingLists.Infrastructure.Database.Source;

internal sealed class MenuPlanSourceRecord
{
    public MenuPlanId Id { get; set; }

    public UserId OwnerUserId { get; set; }

    public List<MenuPlanItemSourceRecord> Items { get; set; } = [];
}
