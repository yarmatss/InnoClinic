using Profiles.IntegrationTests.Infrastructure;
using System.Net.Http.Headers;

namespace Profiles.IntegrationTests.Endpoints;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected readonly PostgresContainerFixture DbFixture;
    protected readonly DatabaseResetter Resetter;
    protected readonly ProfilesApiFactory Factory;
    protected readonly HttpClient Client;

    protected IntegrationTestBase(PostgresContainerFixture dbFixture)
    {
        DbFixture = dbFixture;
        Resetter = new DatabaseResetter(dbFixture.ConnectionString);
        Factory = new ProfilesApiFactory(dbFixture);
        Client = Factory.CreateClient();
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(TestAuthHandler.AuthenticationScheme);
    }

    public async ValueTask InitializeAsync()
    {
        await Resetter.ResetAsync();
    }

    public ValueTask DisposeAsync()
    {
        Client.Dispose();
        Factory.Dispose();
        return ValueTask.CompletedTask;
    }
}
