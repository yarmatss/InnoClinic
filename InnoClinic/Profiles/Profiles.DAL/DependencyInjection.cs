using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Profiles.DAL.Data;

namespace Profiles.DAL;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddDataAccess(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string not found.");

            services.AddDbContext<ProfilesDbContext>(options =>
                options.UseNpgsql(connectionString));

            return services;
        }
    }

    extension(IHost host)
    {
        public async Task ApplyMigrationsAsync()
        {
            using var scope = host.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ProfilesDbContext>();
            await context.Database.MigrateAsync();
        }
    }
}
