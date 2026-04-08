namespace Profiles.API.Extensions;

using Microsoft.Extensions.Logging;

/// <summary>
/// 1000-1999: Request logging;
/// 5000-5999: Critical exceptions
/// </summary>
public static partial class LoggerExtensions
{
    [LoggerMessage(
        EventId = 1000,
        Level = LogLevel.Information,
        Message = "Request {Method} {Path} started | TraceId: {TraceId}")]
    public static partial void LogRequestStarted(
        this ILogger logger,
        string method,
        string path,
        string traceId);

    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Information,
        Message = "Request {Method} {Path} finished with status {StatusCode} in {ElapsedMilliseconds}ms | TraceId: {TraceId}")]
    public static partial void LogRequestFinished(
        this ILogger logger,
        string method,
        string path,
        int statusCode,
        double elapsedMilliseconds,
        string traceId);

    [LoggerMessage(
        EventId = 5000,
        Level = LogLevel.Error,
        Message = "Unhandled exception occurred while processing {Method} {Path}. TraceId: {TraceId}")]
    public static partial void LogCriticalException(
        this ILogger logger,
        Exception exception,
        string method,
        string path,
        string traceId);
}
