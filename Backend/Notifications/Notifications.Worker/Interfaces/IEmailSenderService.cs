namespace Notifications.Worker.Interfaces;

public interface IEmailSenderService
{
    Task SendAsync(
        string recipient,
        string subject,
        string body,
        CancellationToken ct = default);
}
