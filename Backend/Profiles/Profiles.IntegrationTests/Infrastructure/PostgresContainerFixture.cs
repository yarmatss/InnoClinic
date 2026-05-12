using Microsoft.EntityFrameworkCore;
using Profiles.DAL.Data;
using Testcontainers.PostgreSql;

namespace Profiles.IntegrationTests.Infrastructure;

public sealed class PostgresContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18-alpine")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public ProfilesDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ProfilesDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;
        return new ProfilesDbContext(options);
    }

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();
        await using var ctx = CreateDbContext();
        await ctx.Database.MigrateAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}
