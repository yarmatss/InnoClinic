using Azure.Monitor.OpenTelemetry.AspNetCore;
using InnoClinic.AspNetCore.Constants;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using OpenTelemetry.Trace;

namespace InnoClinic.AspNetCore.Extensions;

public static class ObservabilityExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder AddAppObservability(string serviceName)
        {
            builder.Logging.AddConsole();

            var otel = builder.Services.AddOpenTelemetry();

            var appInsightsConnectionString = builder.Configuration[ConnectionConstants.ApplicationInsightsConnectionString];
            if (!string.IsNullOrWhiteSpace(appInsightsConnectionString))
            {
                otel.UseAzureMonitor(options =>
                {
                    options.ConnectionString = appInsightsConnectionString;
                });
            }

            otel.WithTracing(tracing =>
            {
                tracing
                    .AddSource(serviceName)
                    .AddSource(ObservabilityConstants.MassTransitActivitySource)
                    .AddEntityFrameworkCoreInstrumentation()
                    .AddNpgsql()
                    .AddRedisInstrumentation();
            });

            return builder;
        }
    }
}
