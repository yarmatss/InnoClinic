namespace Appointments.Domain.Interfaces;

public interface ICacheService
{
    Task<string?> GetOrSetStringAsync(
        string key,
        Func<CancellationToken, Task<string?>> factory,
        CancellationToken cancellationToken = default);
}
