using InnoClinic.Messaging.Contracts;
using MassTransit;
using Microsoft.Extensions.Options;
using Notifications.Worker.Interfaces;
using Notifications.Worker.Options;

namespace Notifications.Worker.Consumers;

public class StaffCreatedConsumer(
    IEmailSenderService emailSender,
    IOptions<FrontendOptions> frontendOptions,
    ILogger<StaffCreatedConsumer> logger) 
    : IConsumer<StaffCreated>
{
    private readonly FrontendOptions _frontend = frontendOptions.Value;

    public async Task Consume(ConsumeContext<StaffCreated> context)
    {
        var message = context.Message;
        logger.LogInformation("Processing StaffCreated notification for StaffId {StaffId}, Email {Email}", message.StaffId, message.Email);

        var isGoogle = IsGoogleEmail(message.Email);
        var googleLoginUrl = isGoogle ? $"{_frontend.BaseUrl.TrimEnd('/')}/?connection=google-oauth2" : null;

        var (subject, body) = BuildInvitationEmail(message.FirstName, message.LastName, message.InvitationUrl, googleLoginUrl);

        await emailSender.SendAsync(message.Email, subject, body, context.CancellationToken);
    }

    private static (string Subject, string Body) BuildInvitationEmail(
        string firstName,
        string lastName,
        string? invitationUrl,
        string? googleLoginUrl)
    {
        var subject = "InnoClinic Staff Invitation";

        var googleSection = !string.IsNullOrWhiteSpace(googleLoginUrl)
            ? $"""
               <p style="margin: 20px 0;">
                   <a href="{googleLoginUrl}" style="background-color: #4285F4; color: #ffffff; padding: 12px 24px; text-decoration: none; border-radius: 4px; font-weight: bold; display: inline-block;">
                       Sign in with Google
                   </a>
               </p>
               <p>Or, if you prefer to use a standard password:</p>
               """
            : "";

        var passwordSection = !string.IsNullOrWhiteSpace(invitationUrl)
            ? $"<p><a href=\"{invitationUrl}\">Click here to set up your password and access the InnoClinic staff portal</a>.</p>"
            : "<p>Your staff account has been created by an administrator.</p>";

        var body = $"""
            <h2>Hello {firstName} {lastName}!</h2>
            <p>Welcome to the InnoClinic team.</p>
            {googleSection}
            {passwordSection}
            """;

        return (subject, body);
    }

    private static bool IsGoogleEmail(string email) =>
        email.EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase) ||
        email.EndsWith("@googlemail.com", StringComparison.OrdinalIgnoreCase);
}
