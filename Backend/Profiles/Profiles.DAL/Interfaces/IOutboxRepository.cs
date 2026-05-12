using Profiles.DAL.Entities;

namespace Profiles.DAL.Interfaces;

public interface IOutboxRepository : IBaseRepository<OutboxMessage>
{
    Task<List<OutboxMessage>> GetUnprocessedMessagesAsync(
        int batchSize,
        CancellationToken ct);
}
