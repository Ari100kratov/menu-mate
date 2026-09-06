using MenuMate.Common.Application;
using MenuMate.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace MenuMate.Modules.MenuPlanning.Infrastructure.Database;

internal sealed class MenuPlanningUserDataEraser(MenuPlanningDbContext dbContext) : IUserDataEraser
{
    public int Order => 200;

    public async Task EraseAsync(UserId userId, CancellationToken cancellationToken)
    {
        await dbContext.MenuCalendarItems
            .Where(item => item.OwnerUserId == userId)
            .ExecuteDeleteAsync(cancellationToken);
        await dbContext.MealSlots
            .Where(slot => slot.OwnerUserId == userId)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
