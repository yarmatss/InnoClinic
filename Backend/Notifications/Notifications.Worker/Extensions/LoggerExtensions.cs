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

    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Information, 
        Message = "Email successfully sent to {Recipient}")]
    public static partial void LogEmailSent(
        this ILogger logger, 
        string recipient);

    [LoggerMessage(
        EventId = 1003,
        Level = LogLevel.Information,
        Message = "Processing appointment reminder notification for AppointmentId: {AppointmentId}, PatientId: {PatientId}")]
    public static partial void LogAppointmentReminderNotificationProcessing(
        this ILogger logger,
        Guid appointmentId,
        Guid patientId);
}
