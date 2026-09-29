using Notifications.Worker.Models;

namespace Notifications.Worker.Interfaces;

public interface IEmailSenderService
{
    Task SendAsync(
        string recipient,
        EmailContent content,
        CancellationToken ct = default);
}
