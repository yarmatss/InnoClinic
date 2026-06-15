using System.Text.Json;
using InnoClinic.Messaging.Outbox;
using Profiles.DAL.Data;

namespace Profiles.DAL.Repositories;

public class NotificationProducer(ProfilesDbContext dbContext) : INotificationProducer
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
