using MenuMate.Common.Application;
using MenuMate.Modules.MenuPlanning.Application.Abstractions;
using MenuMate.Modules.MenuPlanning.Domain.Models;
using MenuMate.SharedKernel;
using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.MenuPlanning.Application;

internal sealed class DefaultMealSlotRegistrationInitializer(
    IMenuCalendarRepository repository,
    IMenuCalendarUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IUserRegistrationInitializer
{
    private static readonly string[] DefaultNames = ["Завтрак", "Обед", "Ужин"];

    public async Task InitializeAsync(UserId userId, CancellationToken cancellationToken)
    {
        if ((await repository.GetMealSlotsAsync(userId, cancellationToken)).Count > 0)
        {
            return;
        }

        DateTimeOffset now = timeProvider.GetUtcNow();
        for (int index = 0; index < DefaultNames.Length; index++)
        {
            Result<MealSlot> mealSlot = MealSlot.Create(
                Guid.CreateVersion7(),
                userId,
                DefaultNames[index],
                index,
                now);
            if (mealSlot.IsFailure)
            {
                throw new InvalidOperationException(mealSlot.Error.Description);
            }

            await repository.AddMealSlotAsync(mealSlot.Value, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
