using Appointments.Infrastructure.Data;
using InnoClinic.Messaging.Outbox;
using System.Text.Json;

namespace Appointments.Infrastructure.Services;

public class NotificationProducer(AppointmentsDbContext dbContext, TimeProvider timeProvider) : INotificationProducer
{
    public void Enqueue<T>(T payload) where T : class
    {
        var outboxMessage = new NotificationOutbox
        {
            Id = Guid.NewGuid(),
            MessageType = MessageTypeCache<T>.Name,
            Payload = JsonSerializer.Serialize(payload),
            Status = OutboxStatus.Pending,
            CreatedAtUtc = timeProvider.GetUtcNow().UtcDateTime,
            RetryCount = 0
        };

        dbContext.NotificationOutboxes.Add(outboxMessage);
    }
}
