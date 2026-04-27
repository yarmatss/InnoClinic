using Npgsql;
using Respawn;

namespace Profiles.IntegrationTests.Infrastructure;

public sealed class DatabaseResetter(string connectionString)
{
    private Respawner? _respawner;

    public async Task InitializeAsync()
    {
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();
        _respawner = await Respawner.CreateAsync(conn, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"]
        });
    }

    public async Task ResetAsync()
    {
        if (_respawner is null)
        {
            await InitializeAsync();
        }

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();
        await _respawner!.ResetAsync(conn);
    }
}
