using System.Text.Json;
using InnoClinic.Messaging.Outbox;
using Profiles.DAL.Data;

namespace Profiles.DAL.Repositories;

public class NotificationProducer(ProfilesDbContext dbContext, TimeProvider timeProvider) : INotificationProducer
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
