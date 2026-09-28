using InnoClinic.Messaging.Contracts;
using MassTransit;
using Notifications.Worker.Helpers;
using Notifications.Worker.Interfaces;

namespace Notifications.Worker.Consumers;

public class StaffCreatedConsumer(
    IEmailSenderService emailSender,
    EmailTemplateHelper templateHelper,
    ILogger<StaffCreatedConsumer> logger) 
    : IConsumer<StaffCreated>
{
    public async Task Consume(ConsumeContext<StaffCreated> context)
    {
        var message = context.Message;
        logger.LogInformation("Processing StaffCreated notification for StaffId {StaffId}, Email {Email}", message.StaffId, message.Email);

        var (subject, body) = templateHelper.BuildAccountEmail(
            message,
            subject: "InnoClinic Staff Invitation",
            welcomeText: "Welcome to the InnoClinic team.",
            passwordLinkText: "Click here to set up your password and access the InnoClinic staff portal");

        await emailSender.SendAsync(message.Email, subject, body, context.CancellationToken);
    }
}
