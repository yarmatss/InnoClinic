using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Profiles.BLL.Interfaces;
using Profiles.DAL.Data;

namespace Profiles.IntegrationTests.Infrastructure;

public sealed class ProfilesApiFactory(PostgresContainerFixture db)
    : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing"); // Prevents migration on start
        builder.UseSetting("Auth0:Services:ClientId", "test-client");
        builder.UseSetting("Auth0:Services:ClientSecret", "test-secret");

        builder.ConfigureTestServices(services =>
        {
            services.AddAuthentication(defaultScheme: TestAuthHandler.AuthenticationScheme)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.AuthenticationScheme, options => { });

            services.AddAuthorizationBuilder()
                .SetFallbackPolicy(new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build());

            services.RemoveAll<IAuthManagementService>();
            services.AddScoped<IAuthManagementService, FakeAuthManagementService>();

            services.RemoveAll<DbContextOptions<ProfilesDbContext>>();
            services.RemoveAll<ProfilesDbContext>();

            services.AddDbContext<ProfilesDbContext>(options =>
                options.UseNpgsql(db.ConnectionString));
        });
    }
}
