using Microsoft.Extensions.Logging;

namespace InnoClinic.AspNetCore.Extensions;

public static partial class ExceptionHandlerLoggerExtensions
{
    [LoggerMessage(
        EventId = 4000,
        Level = LogLevel.Warning,
        Message = "Bad request payload received at {Path}. TraceId: {TraceId}. Exception: {Message}")]
    public static partial void LogBadRequestPayload(
        this ILogger logger,
        string path,
        string traceId,
        string message);

    [LoggerMessage(
        EventId = 4001,
        Level = LogLevel.Warning,
        Message = "Validation failed at {Path}. TraceId: {TraceId}. Errors: {Errors}")]
    public static partial void LogValidationFailure(
        this ILogger logger,
        string path,
        string traceId,
        string errors);

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
