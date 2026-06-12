using Appointments.Infrastructure.Data;
using InnoClinic.Messaging.Outbox;
using System.Text.Json;

namespace Appointments.Infrastructure.Services;

public class NotificationProducer(AppointmentsDbContext dbContext) : INotificationProducer
{
    public void Enqueue<T>(T payload) where T : class
    {
        var outboxMessage = new NotificationOutbox
        {
            Id = Guid.NewGuid(),
            MessageType = $"{typeof(T).FullName}, {typeof(T).Assembly.GetName().Name}",
            Payload = JsonSerializer.Serialize(payload),
            Status = OutboxStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow,
            RetryCount = 0
        };

        dbContext.NotificationOutboxes.Add(outboxMessage);
    }
}
