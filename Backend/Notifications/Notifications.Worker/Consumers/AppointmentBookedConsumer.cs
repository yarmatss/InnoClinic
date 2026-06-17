using InnoClinic.Messaging.Contracts;
using MassTransit;

namespace Notifications.Worker.Consumers;

public class AppointmentBookedConsumer(ILogger<AppointmentBookedConsumer> logger) 
    : IConsumer<AppointmentBooked>
{
    public Task Consume(ConsumeContext<AppointmentBooked> context)
    {
        var message = context.Message;
        
        logger.LogInformation("Processing appointment booking notification for AppointmentId: {AppointmentId}, PatientId: {PatientId}", 
            message.AppointmentId, message.PatientId);

        // TODO
        
        return Task.CompletedTask;
    }
}
