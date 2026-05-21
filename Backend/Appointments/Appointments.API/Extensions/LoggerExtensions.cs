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
        EventId = 2000,
        Level = LogLevel.Information,
        Message = "Cache miss for medical staff {MedicalStaffId}. Attempting fallback to Profiles service.")]
    public static partial void LogCacheMissFallback(
        this ILogger logger,
        Guid medicalStaffId);

    [LoggerMessage(
        EventId = 2001,
        Level = LogLevel.Information,
        Message = "Successfully re-populated cache for medical staff {MedicalStaffId}.")]
    public static partial void LogCacheRepopulated(
        this ILogger logger,
        Guid medicalStaffId);

    [LoggerMessage(
        EventId = 5000,
        Level = LogLevel.Error,
        Message = "Failed to sync Staff: {StaffId} to Redis.")]
    public static partial void LogSyncError(
        this ILogger logger,
        Exception exception,
        string staffId);

    [LoggerMessage(
        EventId = 5001,
        Level = LogLevel.Error,
        Message = "Fallback gRPC call failed for medical staff {MedicalStaffId}.")]
    public static partial void LogFallbackError(
        this ILogger logger,
        Exception exception,
        Guid medicalStaffId);

    [LoggerMessage(
        EventId = 5002,
        Level = LogLevel.Error,
        Message = "Error booking appointment for medical staff {MedicalStaffId}.")]
    public static partial void LogBookingError(
        this ILogger logger,
        Exception exception,
        Guid medicalStaffId);
}
