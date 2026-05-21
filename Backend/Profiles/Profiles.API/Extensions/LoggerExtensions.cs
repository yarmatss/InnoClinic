namespace Profiles.API.Extensions;

using Microsoft.Extensions.Logging;

/// <summary>
/// 1000-1999: Request logging;
/// 2000-2999: Background jobs;
/// 4000-4999: Warning and handled exceptions;
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
        EventId = 2000,
        Level = LogLevel.Information,
        Message = "Outbox Processor Background Service is starting.")]
    public static partial void LogOutboxProcessorStarting(this ILogger logger);

    [LoggerMessage(
        EventId = 2001,
        Level = LogLevel.Information,
        Message = "Successfully synced Staff ID: {StaffId}")]
    public static partial void LogOutboxSyncSuccess(
        this ILogger logger,
        string staffId);

    [LoggerMessage(
        EventId = 2002,
        Level = LogLevel.Information,
        Message = "Processing GetStaffProfile request for MedicalStaffId: {MedicalStaffId}")]
    public static partial void LogProcessingGetStaffProfile(
        this ILogger logger,
        string medicalStaffId);

    [LoggerMessage(
        EventId = 4001,
        Level = LogLevel.Warning,
        Message = "gRPC network error for Outbox Message {Id}. Retrying later.")]
    public static partial void LogOutboxGrpcError(
        this ILogger logger,
        Exception exception,
        Guid id);

    [LoggerMessage(
        EventId = 5001,
        Level = LogLevel.Error,
        Message = "Failed to process Outbox Message {Id}")]
    public static partial void LogOutboxProcessingError(
        this ILogger logger,
        Exception exception,
        Guid id);

    [LoggerMessage(
        EventId = 5002,
        Level = LogLevel.Error,
        Message = "A fatal error occurred while processing the outbox.")]
    public static partial void LogOutboxFatalError(
        this ILogger logger,
        Exception exception);
}
