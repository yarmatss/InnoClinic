using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using Profiles.DAL.Data;

namespace Profiles.DAL;

public static class DependencyInjection
{
    public static IServiceCollection AddDataAccess(this IServiceCollection services, IConfiguration configuration)
    {
        var baseConnectionString = configuration.GetConnectionString("DefaultConnection");

        var builder = new NpgsqlConnectionStringBuilder(baseConnectionString)
        {
            Password = configuration["DbPassword"]
        };

        services.AddDbContext<ProfilesDbContext>(options =>
            options.UseNpgsql(builder.ConnectionString));

        return services;
    }

    public static async Task ApplyMigrationsAsync(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ProfilesDbContext>();
        await context.Database.MigrateAsync();
    }
}
