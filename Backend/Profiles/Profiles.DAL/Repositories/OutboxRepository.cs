using Microsoft.EntityFrameworkCore;
using Profiles.DAL.Data;
using Profiles.DAL.Entities;
using Profiles.DAL.Interfaces;

namespace Profiles.DAL.Repositories;

public class OutboxRepository(ProfilesDbContext context) :
    BaseRepository<OutboxMessage>(context), IOutboxRepository
{
    public async Task<List<OutboxMessage>> GetUnprocessedMessagesAsync(
        int batchSize,
        CancellationToken ct)
    {
        return await GetQuery(trackChanges: true)
            .Where(m => m.ProcessedOnUtc == null)
            .OrderBy(m => m.OccurredOnUtc)
            .Take(batchSize)
            .ToListAsync(ct);
    }
}
