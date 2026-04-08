namespace Profiles.API.Extensions;

using Microsoft.Extensions.Logging;

public static partial class LoggerExtensions
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Request {Method} {Path} started")]
    public static partial void LogRequestStarted(
        this ILogger logger,
        string method,
        string path);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Information,
        Message = "Request {Method} {Path} finished with status {StatusCode} in {ElapsedMilliseconds}ms")]
    public static partial void LogRequestFinished(
        this ILogger logger,
        string method,
        string path,
        int statusCode,
        double elapsedMilliseconds);
}
