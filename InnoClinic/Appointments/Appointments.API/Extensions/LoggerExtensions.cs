namespace Appointments.API.Extensions;

using Microsoft.Extensions.Logging;

public static partial class LoggerExtensions
{
    [LoggerMessage(
        EventId = 1000,
        Level = LogLevel.Information,
        Message = "Processing sync for Medical Staff: {StaffId}")]
    public static partial void LogProcessingSync(
        this ILogger logger,
        string staffId);

    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Information,
        Message = "Staff {StaffId} deactivated. Entry removed from Redis.")]
    public static partial void LogStaffDeactivated(
        this ILogger logger,
        string staffId);

    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Information,
        Message = "Successfully updated Redis for Staff: {StaffId}")]
    public static partial void LogSyncSuccess(
        this ILogger logger,
        string staffId);

    [LoggerMessage(
        EventId = 5000,
        Level = LogLevel.Error,
        Message = "Failed to sync Staff: {StaffId} to Redis.")]
    public static partial void LogSyncError(
        this ILogger logger,
        Exception exception,
        string staffId);
}
