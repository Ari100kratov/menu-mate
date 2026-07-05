using MenuMate.Common.Application;
using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.DataImporter;

internal sealed class ImporterUserContext : IUserContext
{
    private UserId? _userId;

    public UserId UserId => _userId ??
        throw new InvalidOperationException("Администратор для импорта еще не определен.");

    public void SetUser(UserId userId) => _userId = userId;
}
