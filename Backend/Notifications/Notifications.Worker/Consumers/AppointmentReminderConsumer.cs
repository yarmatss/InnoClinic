using InnoClinic.Messaging.Contracts;
using MassTransit;
using Notifications.Worker.Extensions;
using Notifications.Worker.Interfaces;
using Notifications.Worker.Services;

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

        var emailContent = EmailComposer.ComposeAppointmentReminderEmail(
            message.PatientName,
            message.MedicalStaffName,
            message.StartTime);

        await emailSender.SendAsync(message.PatientEmail, emailContent, context.CancellationToken);
    }
}
