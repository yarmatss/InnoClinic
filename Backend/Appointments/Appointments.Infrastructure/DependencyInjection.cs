using Appointments.Domain.Constants;
using Appointments.Domain.Interfaces;
using Appointments.Infrastructure.Caching;
using Appointments.Infrastructure.Connection;
using Appointments.Infrastructure.Data;
using Appointments.Infrastructure.Interceptors;
using InnoClinic.Contracts.Grpc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using System.Net.Security;

namespace Appointments.Infrastructure;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddInfrastructure(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString(ConnectionConstants.DefaultConnection)
                ?? throw new InvalidOperationException("Connection string not found.");

            services.AddSingleton<PostgresExceptionInterceptor>();

            services.AddDbContext<AppointmentsDbContext>((sp, options) =>
            {
                options.UseNpgsql(connectionString)
                    .AddInterceptors(sp.GetRequiredService<PostgresExceptionInterceptor>());
            });

            services.AddSingleton<ISqlConnectionFactory>(_ =>
                new SqlConnectionFactory(connectionString));

            var redisConnectionString = configuration.GetConnectionString(ConnectionConstants.RedisConnection)
                ?? throw new InvalidOperationException("Redis connection string not found.");

            services.AddSingleton<IConnectionMultiplexer>(_ =>
                ConnectionMultiplexer.Connect(redisConnectionString));

            services.AddSingleton<ICacheService, RedisCacheService>();

            services.AddGrpcClient<StaffScheduleSyncService.StaffScheduleSyncServiceClient>(options =>
            {
                var profilesApiUrl = configuration[ConnectionConstants.ProfilesApiUrl]
                    ?? throw new InvalidOperationException($"{ConnectionConstants.ProfilesApiUrl} not found in configuration.");

                options.Address = new Uri(profilesApiUrl);
            })
            .ConfigurePrimaryHttpMessageHandler(() => CreateSocketsHttpHandler(configuration));

            services.AddGrpcClient<PatientService.PatientServiceClient>(options =>
            {
                var profilesApiUrl = configuration[ConnectionConstants.ProfilesApiUrl]
                    ?? throw new InvalidOperationException($"{ConnectionConstants.ProfilesApiUrl} not found in configuration.");

                options.Address = new Uri(profilesApiUrl);
            })
            .ConfigurePrimaryHttpMessageHandler(() => CreateSocketsHttpHandler(configuration));

            return services;
        }
    }

    private static SocketsHttpHandler CreateSocketsHttpHandler(IConfiguration configuration)
    {
        return new SocketsHttpHandler
        {
            EnableMultipleHttp2Connections = true,
            PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
            KeepAlivePingDelay = TimeSpan.FromSeconds(60),
            KeepAlivePingTimeout = TimeSpan.FromSeconds(30),
            SslOptions = new SslClientAuthenticationOptions
            {
                RemoteCertificateValidationCallback = (sender, cert, chain, sslPolicyErrors) =>
                {
                    if (sslPolicyErrors == SslPolicyErrors.None)
                        return true;

                    return configuration["ASPNETCORE_ENVIRONMENT"] == "Development";
                }
            }
        };
    }

    extension(IHost host)
    {
        public async Task ApplyMigrationsAsync()
        {
            using var scope = host.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppointmentsDbContext>();
            await context.Database.MigrateAsync();
        }
    }
}
