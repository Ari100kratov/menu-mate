namespace MenuMate.Api.IntegrationTests;

internal static class TestEmail
{
    public static string Create(string prefix) =>
        $"{prefix}-{Guid.CreateVersion7():N}@example.test";
}
