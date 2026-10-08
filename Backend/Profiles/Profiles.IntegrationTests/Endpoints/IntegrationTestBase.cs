using Profiles.DAL.Entities;
using Profiles.Domain.Enums;
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

        await using var ctx = DbFixture.CreateDbContext();
        var admin = new MedicalStaff
        {
            Id = Guid.NewGuid(),
            UserId = "test-user-id",
            StaffType = StaffType.Administrator,
            Email = "admin@test.com",
            FirstName = "Test",
            LastName = "Admin",
            ContactPhone = "1234567890",
            LicenseNumber = "TEST-ADMIN-01",
            IsActive = true,
            HireDate = new DateOnly(2020, 1, 1),
            Gender = Gender.Male,
            NationalId = "12345678901",
            BirthDate = new DateOnly(1980, 1, 1)
        };
        ctx.Staff.Add(admin);
        await ctx.SaveChangesAsync();
    }

    public ValueTask DisposeAsync()
    {
        Client.Dispose();
        Factory.Dispose();
        return ValueTask.CompletedTask;
    }
}
