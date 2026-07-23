using MenuMate.Common.Application;
using MenuMate.Contracts.Auth;

namespace MenuMate.Modules.Auth.Application.GetAdminUsers;

/// <summary>
/// Запрос постраничного списка зарегистрированных пользователей для администратора.
/// </summary>
/// <param name="Search">Поисковая строка по имени или email.</param>
/// <param name="Page">Номер страницы, начиная с единицы.</param>
/// <param name="PageSize">Количество пользователей на странице.</param>
public sealed record GetAdminUsersQuery(string? Search, int Page, int PageSize)
    : IQuery<AdminUsersPageResponse>;
