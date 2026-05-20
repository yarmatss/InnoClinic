using Appointments.Domain.Interfaces;
using StackExchange.Redis;
using System.Collections.Concurrent;

namespace Appointments.Infrastructure.Caching;

public class RedisCacheService(IConnectionMultiplexer redis) : ICacheService
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    public async Task<string?> GetOrSetStringAsync(
        string key,
        Func<CancellationToken, Task<string?>> factory,
        CancellationToken cancellationToken = default)
    {
        var db = redis.GetDatabase();
        
        var cachedValue = await db.StringGetAsync(key);
        if (!cachedValue.IsNullOrEmpty)
        {
            return cachedValue;
        }

        var semaphore = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync(cancellationToken);

        try
        {
            cachedValue = await db.StringGetAsync(key);
            if (!cachedValue.IsNullOrEmpty)
            {
                return cachedValue;
            }

            var result = await factory(cancellationToken);

            if (result != null)
            {
                await db.StringSetAsync(key, result);
            }

            return result;
        }
        finally
        {
            semaphore.Release();
        }
    }
}
