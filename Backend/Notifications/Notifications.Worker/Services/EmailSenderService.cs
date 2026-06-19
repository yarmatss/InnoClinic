using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Notifications.Worker.Constants;
using Notifications.Worker.Extensions;
using Notifications.Worker.Interfaces;
using Notifications.Worker.Options;
using Polly;
using Polly.Registry;

namespace Notifications.Worker.Services;

public partial class EmailSenderService(
    IOptions<EmailOptions> options,
    ResiliencePipelineProvider<string> pipelineProvider,
    ILogger<EmailSenderService> logger) 
    : IEmailSenderService
{
    private readonly EmailOptions _options = options.Value;
    private readonly ResiliencePipeline _pipeline = pipelineProvider.GetPipeline(NotificationConstants.EmailRetryPipeline);

    public async Task SendAsync(
        string recipient,
        string subject,
        string body,
        CancellationToken ct = default)
    {
        await _pipeline.ExecuteAsync(async token =>
        {
            using var client = new SmtpClient();

            var socketOptions = _options.UseSsl
                ? SecureSocketOptions.SslOnConnect
                : SecureSocketOptions.Auto;

            await client.ConnectAsync(_options.Host, _options.Port, socketOptions, token);

            if (!string.IsNullOrEmpty(_options.UserName) && !string.IsNullOrEmpty(_options.Password))
                await client.AuthenticateAsync(_options.UserName, _options.Password, token);

            using var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_options.SenderName, _options.SenderEmail));
            message.To.Add(MailboxAddress.Parse(recipient));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = body };
            message.Body = bodyBuilder.ToMessageBody();

            await client.SendAsync(message, token);
            await client.DisconnectAsync(true, token);
            
            logger.LogEmailSent(recipient);
        }, ct);
    }
}
