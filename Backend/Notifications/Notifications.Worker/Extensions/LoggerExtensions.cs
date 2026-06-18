namespace Notifications.Worker.Extensions;

public static partial class LoggerExtensions
{
    [LoggerMessage(
        EventId = 1000,
        Level = LogLevel.Information,
        Message = "Processing appointment booking notification for AppointmentId: {AppointmentId}, PatientId: {PatientId}")]
    public static partial void LogAppointmentBookingNotificationProcessing(
        this ILogger logger,
        Guid appointmentId,
        Guid patientId);

    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Information,
        Message = "Processing patient creation notification for PatientId: {PatientId}, Email: {Email}")]
    public static partial void LogPatientCreationNotificationProcessing(
        this ILogger logger,
        Guid patientId,
        string email);
}
