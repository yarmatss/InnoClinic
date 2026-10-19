using Appointments.Domain.Constants;
using Appointments.Domain.Interfaces;
using Appointments.Infrastructure.Caching;
using Appointments.Infrastructure.Connection;
using Appointments.Infrastructure.Data;
using Appointments.Infrastructure.Interceptors;
using Appointments.Infrastructure.Services;
using InnoClinic.Contracts.Grpc;
using InnoClinic.Contracts.Grpc.Auth;
using InnoClinic.Messaging.Outbox;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;

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
            services.AddSingleton(TimeProvider.System);

            services.AddDbContext<AppointmentsDbContext>((sp, options) =>
            {
                options.UseNpgsql(connectionString)
                    .AddInterceptors(sp.GetRequiredService<PostgresExceptionInterceptor>());
            });

            services.AddScoped<INotificationProducer, NotificationProducer>();

            services.AddSingleton<ISqlConnectionFactory>(_ =>
                new SqlConnectionFactory(connectionString));

            var redisConnectionString = configuration.GetConnectionString(ConnectionConstants.RedisConnection)
                ?? throw new InvalidOperationException("Redis connection string not found.");

            services.AddSingleton<IConnectionMultiplexer>(_ =>
                ConnectionMultiplexer.Connect(redisConnectionString));

            services.AddSingleton<ICacheService, RedisCacheService>();

            var rabbitMqConnectionString = configuration.GetConnectionString(ConnectionConstants.RabbitMQConnection)
                ?? throw new InvalidOperationException("RabbitMQ connection string not found.");

            services.AddMassTransit(x =>
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(rabbitMqConnectionString);
                });
            });

            var profilesApiUrl = configuration[ConnectionConstants.ProfilesApiUrl]
                ?? throw new InvalidOperationException($"{ConnectionConstants.ProfilesApiUrl} not found in configuration.");

            services.AddInternalGrpcClient<StaffScheduleSyncService.StaffScheduleSyncServiceClient>(profilesApiUrl, configuration);
            services.AddInternalGrpcClient<PatientService.PatientServiceClient>(profilesApiUrl, configuration);

            return services;
        }
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
