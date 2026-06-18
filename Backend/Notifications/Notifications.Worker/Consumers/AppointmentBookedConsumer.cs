using InnoClinic.Messaging.Contracts;
using MassTransit;
using Notifications.Worker.Extensions;

namespace Notifications.Worker.Consumers;

public class AppointmentBookedConsumer(ILogger<AppointmentBookedConsumer> logger) 
    : IConsumer<AppointmentBooked>
{
    public Task Consume(ConsumeContext<AppointmentBooked> context)
    {
        var message = context.Message;
        logger.LogAppointmentBookingNotificationProcessing(message.AppointmentId, message.PatientId);
        return Task.CompletedTask;
    }
}
