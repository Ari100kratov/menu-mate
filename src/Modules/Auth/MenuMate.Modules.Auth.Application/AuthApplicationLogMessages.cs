using Microsoft.Extensions.Logging;

namespace MenuMate.Modules.Auth.Application;

internal static partial class AuthApplicationLogMessages
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Warning,
        Message = "Transactional email delivery failed during {Operation}.")]
    public static partial void EmailDeliveryFailed(ILogger logger, string operation, Exception exception);
}
