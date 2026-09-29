using InnoClinic.Messaging.Contracts;
using MassTransit;
using Notifications.Worker.Interfaces;
using Notifications.Worker.Services;

namespace Notifications.Worker.Consumers;

public class StaffCreatedConsumer(
    IEmailSenderService emailSender,
    EmailComposer emailComposer,
    ILogger<StaffCreatedConsumer> logger) 
    : IConsumer<StaffCreated>
{
    public async Task Consume(ConsumeContext<StaffCreated> context)
    {
        var message = context.Message;
        logger.LogInformation("Processing StaffCreated notification for StaffId {StaffId}, Email {Email}", message.StaffId, message.Email);

        var emailContent = emailComposer.ComposeAccountEmail(
            message,
            subject: "InnoClinic Staff Invitation",
            welcomeText: "Welcome to the InnoClinic team.",
            passwordLinkText: "Click here to set up your password and access the InnoClinic staff portal");

        await emailSender.SendAsync(message.Email, emailContent, context.CancellationToken);
    }
}
