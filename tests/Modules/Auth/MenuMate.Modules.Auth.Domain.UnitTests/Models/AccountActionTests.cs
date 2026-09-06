using MenuMate.Modules.Auth.Domain.Models;

namespace MenuMate.Modules.Auth.Domain.UnitTests.Models;

public sealed class AccountActionTests
{
    private static readonly DateTimeOffset FixedNow = new(2026, 8, 23, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void ActionShouldExpireAndBecomeSingleUse()
    {
        AccountAction action = CreateAction();

        Assert.True(action.CanAttempt(FixedNow));
        Assert.False(action.CanAttempt(FixedNow.AddMinutes(16)));

        action.MarkUsed(FixedNow.AddMinutes(1));

        Assert.False(action.CanAttempt(FixedNow.AddMinutes(2)));
    }

    [Fact]
    public void ActionShouldStopAfterFiveFailedAttempts()
    {
        AccountAction action = CreateAction();

        for (int attempt = 0; attempt < AccountAction.MaximumFailedAttempts; attempt++)
        {
            action.RegisterFailedAttempt();
        }

        Assert.Equal(AccountAction.MaximumFailedAttempts, action.FailedAttempts);
        Assert.False(action.CanAttempt(FixedNow));
    }

    private static AccountAction CreateAction() => AccountAction.Create(
        Guid.CreateVersion7(),
        Guid.CreateVersion7(),
        AccountActionPurpose.RegistrationEmailVerification,
        "user@example.com",
        "hash",
        FixedNow,
        FixedNow.AddMinutes(15),
        FixedNow.AddMinutes(1));
}
