using InnoClinic.Messaging.Contracts;
using MassTransit;

namespace Notifications.Worker.Consumers;

public class PatientCreatedConsumer(ILogger<PatientCreatedConsumer> logger) 
    : IConsumer<PatientCreated>
{
    public Task Consume(ConsumeContext<PatientCreated> context)
    {
        var message = context.Message;

        logger.LogInformation("Processing patient creation notification for PatientId: {PatientId}, Email: {Email}",
            message.PatientId, message.Email);

        // TODO

        return Task.CompletedTask;
    }
}
