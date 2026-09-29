using InnoClinic.Messaging.Contracts;
using MassTransit;
using Notifications.Worker.Extensions;
using Notifications.Worker.Interfaces;
using Notifications.Worker.Services;

namespace Notifications.Worker.Consumers;

public class PatientCreatedConsumer(
    IEmailSenderService emailSender,
    EmailComposer emailComposer,
    ILogger<PatientCreatedConsumer> logger) 
    : IConsumer<PatientCreated>
{
    public async Task Consume(ConsumeContext<PatientCreated> context)
    {
        var message = context.Message;
        logger.LogPatientCreationNotificationProcessing(message.PatientId, message.Email);

        var emailContent = emailComposer.ComposeAccountEmail(
            message,
            subject: "Welcome to InnoClinic!",
            welcomeText: "Welcome to InnoClinic. Your patient profile is ready.",
            passwordLinkText: "Click here to set up your password");

        await emailSender.SendAsync(message.Email, emailContent, context.CancellationToken);
    }
}
