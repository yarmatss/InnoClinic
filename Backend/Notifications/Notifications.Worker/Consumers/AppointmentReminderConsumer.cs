using InnoClinic.Messaging.Contracts;
using MassTransit;
using Notifications.Worker.Extensions;
using Notifications.Worker.Helpers;
using Notifications.Worker.Interfaces;

namespace Notifications.Worker.Consumers;

public class AppointmentReminderConsumer(
    IEmailSenderService emailSender,
    EmailTemplateHelper templateHelper,
    ILogger<AppointmentReminderConsumer> logger) 
    : IConsumer<AppointmentReminder>
{
    public async Task Consume(ConsumeContext<AppointmentReminder> context)
    {
        var message = context.Message;

        logger.LogAppointmentReminderNotificationProcessing(message.AppointmentId, message.PatientId);

        var (subject, body) = templateHelper.BuildAppointmentReminderEmail(
            message.PatientName,
            message.MedicalStaffName,
            message.StartTime);

        await emailSender.SendAsync(message.PatientEmail, subject, body, context.CancellationToken);
    }
}
