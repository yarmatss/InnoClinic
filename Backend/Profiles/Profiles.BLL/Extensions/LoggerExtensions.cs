using Microsoft.Extensions.Logging;

namespace Profiles.BLL.Extensions;

internal static partial class LoggerExtensions
{
    [LoggerMessage(
        EventId = 5100,
        Level = LogLevel.Critical,
        Message = "Failed to revert the roles of Auth0 user {UserId}; fix its roles manually.")]
    public static partial void LogRoleRevertFailed(this ILogger logger, Exception exception, string userId);
}
