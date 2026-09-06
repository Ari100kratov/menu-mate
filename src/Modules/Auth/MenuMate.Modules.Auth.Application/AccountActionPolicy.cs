namespace MenuMate.Modules.Auth.Application;

internal static class AccountActionPolicy
{
    public static readonly TimeSpan VerificationCodeLifetime = TimeSpan.FromMinutes(15);
    public static readonly TimeSpan PasswordResetLifetime = TimeSpan.FromMinutes(30);
    public static readonly TimeSpan ResendCooldown = TimeSpan.FromMinutes(1);
}
