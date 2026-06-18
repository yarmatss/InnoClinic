using InnoClinic.Messaging.Contracts;
using MassTransit;
using Notifications.Worker.Extensions;

namespace Notifications.Worker.Consumers;

public class PatientCreatedConsumer(ILogger<PatientCreatedConsumer> logger) 
    : IConsumer<PatientCreated>
{
    public Task Consume(ConsumeContext<PatientCreated> context)
    {
        var message = context.Message;
        logger.LogPatientCreationNotificationProcessing(message.PatientId, message.Email);
        return Task.CompletedTask;
    }
}
