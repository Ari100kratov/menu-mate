using MenuMate.Common.Application;
using MenuMate.Common.Application.Statistics;
using MenuMate.Contracts.Auth;
using MenuMate.Modules.Auth.Application.Abstractions;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.Auth.Application.GetAdminUsers;

/// <summary>
/// Обрабатывает выдачу пользователей для административного раздела.
/// </summary>
internal sealed class GetAdminUsersQueryHandler(
    IAuthReadDbContext dbContext,
    IUserRecipeStatisticsReader recipeStatisticsReader)
    : IQueryHandler<GetAdminUsersQuery, AdminUsersPageResponse>
{
    public async Task<Result<AdminUsersPageResponse>> Handle(
        GetAdminUsersQuery query,
        CancellationToken cancellationToken)
    {
        int page = Math.Clamp(query.Page, 1, 100_000);
        int pageSize = Math.Clamp(query.PageSize, 1, 100);
        AdminUsersPageReadModel usersPage = await dbContext.GetAdminUsersAsync(
            query.Search,
            (page - 1) * pageSize,
            pageSize,
            cancellationToken);
        IReadOnlyDictionary<Guid, int> recipeCounts =
            await recipeStatisticsReader.GetActiveRecipeCountsByOwnerAsync(
                usersPage.Users.Select(user => user.Id).ToArray(),
                cancellationToken);

        var response = new AdminUsersPageResponse(
            usersPage.Users
                .Select(user => new AdminUserListItemResponse(
                    user.Id,
                    user.Email,
                    user.DisplayName,
                    user.RegisteredAt,
                    user.Roles,
                    recipeCounts.GetValueOrDefault(user.Id)))
                .ToArray(),
            usersPage.TotalCount,
            page,
            pageSize);

        return Result.Success(response);
    }
}
