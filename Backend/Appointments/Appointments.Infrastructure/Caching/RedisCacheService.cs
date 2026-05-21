using Appointments.Domain.Interfaces;
using StackExchange.Redis;

namespace Appointments.Infrastructure.Caching;

public class RedisCacheService(IConnectionMultiplexer redis) : ICacheService
{
    private static readonly SemaphoreSlim[] _locks = Enumerable.Range(0, 1024)
        .Select(_ => new SemaphoreSlim(1, 1))
        .ToArray();

    public async Task<string?> GetOrSetStringAsync(
        string key,
        Func<CancellationToken, Task<string?>> factory,
        CancellationToken cancellationToken = default)
    {
        var db = redis.GetDatabase();

        if (await db.StringGetAsync(key) is { IsNullOrEmpty: false } cachedValue)
        {
            return cachedValue;
        }

        var semaphore = _locks[(uint)key.GetHashCode() % _locks.Length];
        await semaphore.WaitAsync(cancellationToken);

        try
        {
            if (await db.StringGetAsync(key) is { IsNullOrEmpty: false } retryValue)
            {
                return retryValue;
            }

            if (await factory(cancellationToken) is { } result)
            {
                await db.StringSetAsync(key, result);
                return result;
            }

            return null;
        }
        finally
        {
            semaphore.Release();
        }
    }
}
