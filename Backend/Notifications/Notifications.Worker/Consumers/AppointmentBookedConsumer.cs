using InnoClinic.Messaging.Contracts;
using MassTransit;
using Notifications.Worker.Extensions;
using Notifications.Worker.Helpers;
using Notifications.Worker.Interfaces;

namespace Notifications.Worker.Consumers;

public class AppointmentBookedConsumer(
    IEmailSenderService emailSender,
    EmailTemplateHelper templateHelper,
    ILogger<AppointmentBookedConsumer> logger) 
    : IConsumer<AppointmentBooked>
{
    public async Task Consume(ConsumeContext<AppointmentBooked> context)
    {
        var message = context.Message;

        logger.LogAppointmentBookingNotificationProcessing(message.AppointmentId, message.PatientId);

        var (subject, body) = templateHelper.BuildAppointmentBookedEmail(
            message.PatientName,
            message.MedicalStaffName,
            message.StartTime);

        await emailSender.SendAsync(message.PatientEmail, subject, body, context.CancellationToken);
    }
}
