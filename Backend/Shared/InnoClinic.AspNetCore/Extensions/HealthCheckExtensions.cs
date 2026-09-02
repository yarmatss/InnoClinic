using InnoClinic.AspNetCore.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using RabbitMQ.Client;

namespace InnoClinic.AspNetCore.Extensions;

public static class HealthCheckExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddAppHealthChecks(IConfiguration configuration)
        {
            var healthBuilder = services.AddHealthChecks()
                .AddCheck(HealthCheckConstants.Self, () => HealthCheckResult.Healthy(), tags: [HealthCheckConstants.LiveTag]);

            var dbConnection = configuration.GetConnectionString(ConnectionConstants.DefaultConnection);
            if (!string.IsNullOrWhiteSpace(dbConnection))
            {
                healthBuilder.AddNpgSql(dbConnection, name: HealthCheckConstants.Postgres, tags: [HealthCheckConstants.ReadyTag]);
            }

            var redisConnection = configuration.GetConnectionString(ConnectionConstants.RedisConnectionString);
            if (!string.IsNullOrWhiteSpace(redisConnection))
            {
                healthBuilder.AddRedis(redisConnection, name: HealthCheckConstants.Redis, tags: [HealthCheckConstants.ReadyTag]);
            }

            var rabbitConnection = configuration.GetConnectionString(ConnectionConstants.RabbitMQConnectionString);
            if (!string.IsNullOrWhiteSpace(rabbitConnection))
            {
                services.AddSingleton(sp =>
                {
                    var factory = new ConnectionFactory
                    {
                        Uri = new Uri(rabbitConnection)
                    };
                    return factory.CreateConnectionAsync().GetAwaiter().GetResult();
                });

                healthBuilder.AddRabbitMQ(name: HealthCheckConstants.CloudAmqp, tags: [HealthCheckConstants.ReadyTag]);
            }

            return services;
        }
    }

    extension(WebApplication app)
    {
        public WebApplication MapAppHealthChecks()
        {
            app.MapHealthChecks(HealthCheckConstants.LiveEndpoint, new HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains(HealthCheckConstants.LiveTag)
            });

            app.MapHealthChecks(HealthCheckConstants.ReadyEndpoint, new HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains(HealthCheckConstants.ReadyTag)
            });

            return app;
        }
    }
}
