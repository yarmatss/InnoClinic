using InnoClinic.Messaging.Contracts;
using MassTransit;
using Notifications.Worker.Extensions;
using Notifications.Worker.Interfaces;

namespace Notifications.Worker.Consumers;

public class PatientCreatedConsumer(
    IEmailSenderService emailSender,
    ILogger<PatientCreatedConsumer> logger) 
    : IConsumer<PatientCreated>
{
    public async Task Consume(ConsumeContext<PatientCreated> context)
    {
        var message = context.Message;
        logger.LogPatientCreationNotificationProcessing(message.PatientId, message.Email);

        var (subject, body) = BuildWelcomeEmail(message.FirstName, message.LastName);

        await emailSender.SendAsync(message.Email, subject, body, context.CancellationToken);
    }

    private static (string Subject, string Body) BuildWelcomeEmail(string firstName, string lastName)
    {
        var subject = "Welcome to InnoClinic!";
        var body = $"<h1>Hello {firstName} {lastName}!</h1><p>Your account has been successfully created.</p>";

        return (subject, body);
    }
}
