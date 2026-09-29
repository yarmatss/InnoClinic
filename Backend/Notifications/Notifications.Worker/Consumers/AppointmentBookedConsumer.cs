using InnoClinic.Messaging.Contracts;
using MassTransit;
using Notifications.Worker.Extensions;
using Notifications.Worker.Interfaces;
using Notifications.Worker.Services;

namespace Notifications.Worker.Consumers;

public class AppointmentBookedConsumer(
    IEmailSenderService emailSender,
    ILogger<AppointmentBookedConsumer> logger) 
    : IConsumer<AppointmentBooked>
{
    public async Task Consume(ConsumeContext<AppointmentBooked> context)
    {
        var message = context.Message;

        logger.LogAppointmentBookingNotificationProcessing(message.AppointmentId, message.PatientId);

        var emailContent = EmailComposer.ComposeAppointmentBookedEmail(
            message.PatientName,
            message.MedicalStaffName,
            message.StartTime);

        await emailSender.SendAsync(message.PatientEmail, emailContent, context.CancellationToken);
    }
}
