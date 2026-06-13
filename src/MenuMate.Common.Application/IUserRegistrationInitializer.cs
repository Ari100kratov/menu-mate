using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Common.Application;

/// <summary>
/// Initializes module-owned data for a newly registered user.
/// </summary>
public interface IUserRegistrationInitializer
{
    /// <summary>
    /// Initializes module-owned data for the user.
    /// </summary>
    Task InitializeAsync(UserId userId, CancellationToken cancellationToken);
}
