using MenuMate.Contracts.MenuPlanning;
using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.MenuPlanning.Application.Abstractions;

/// <summary>
/// Контракт чтения планов меню через EF-проекции без гидрации доменных агрегатов.
/// </summary>
internal interface IMenuPlansReadDbContext
{
    /// <summary>
    /// Возвращает список планов меню.
    /// </summary>
    Task<IReadOnlyCollection<MenuPlanResponse>> GetMenuPlansAsync(
        UserId ownerUserId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Возвращает план меню по идентификатору.
    /// </summary>
    Task<MenuPlanResponse?> GetMenuPlanAsync(
        Guid menuPlanId,
        UserId ownerUserId,
        CancellationToken cancellationToken);
}
