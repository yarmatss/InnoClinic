using InnoClinic.Messaging.Contracts;
using MassTransit;
using Notifications.Worker.Extensions;
using Notifications.Worker.Interfaces;

namespace Notifications.Worker.Consumers;

public class AppointmentReminderConsumer(
    IEmailSenderService emailSender,
    ILogger<AppointmentReminderConsumer> logger) 
    : IConsumer<AppointmentReminder>
{
    public async Task Consume(ConsumeContext<AppointmentReminder> context)
    {
        var message = context.Message;

        logger.LogAppointmentReminderNotificationProcessing(message.AppointmentId, message.PatientId);

        var (subject, body) = BuildReminderEmail(
            message.PatientName,
            message.MedicalStaffName,
            message.StartTime);

        await emailSender.SendAsync(message.PatientEmail, subject, body, context.CancellationToken);
    }

    private static (string Subject, string Body) BuildReminderEmail(
        string patientName,
        string medicalStaffName,
        DateTime startTime)
    {
        var subject = "Reminder: Upcoming Appointment - InnoClinic";

        var formattedDate = startTime.ToString("f");

        var body = $"""
            <h1>Appointment Reminder</h1>
            <p>Dear {patientName},</p>
            <p>This is a friendly reminder that you have an upcoming appointment scheduled with InnoClinic:</p>
            <hr />
            <ul>
                <li><strong>Doctor / Specialist:</strong> {medicalStaffName}</li>
                <li><strong>Date & Time:</strong> {formattedDate} (UTC)</li>
            </ul>
            <p>If you need to reschedule or cancel, please contact the clinic or manage your appointment through the patient portal.</p>
            """;

        return (subject, body);
    }
}
