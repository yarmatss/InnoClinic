using Appointments.Infrastructure.Data;
using Appointments.Infrastructure.Services;
using InnoClinic.Contracts.Grpc;
using InnoClinic.Messaging.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Security;

namespace Appointments.Functions;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddDatabase(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            services.AddDbContext<AppointmentsDbContext>(options =>
            {
                options.UseNpgsql(connectionString);
            });

            return services;
        }

        public IServiceCollection AddOutbox()
        {
            services.AddSingleton(TimeProvider.System);
            services.AddScoped<INotificationProducer, NotificationProducer>();
            return services;
        }

        public IServiceCollection AddProfilesGrpcClients(IConfiguration configuration)
        {
            var profilesApiUrl = configuration["ProfilesApiUrl"]
                ?? throw new InvalidOperationException("Configuration 'ProfilesApiUrl' not found.");

            services.AddGrpcClient<PatientService.PatientServiceClient>(options =>
            {
                options.Address = new Uri(profilesApiUrl);
            })
            .ConfigurePrimaryHttpMessageHandler(() => CreateSocketsHttpHandler(configuration));

            services.AddGrpcClient<StaffScheduleSyncService.StaffScheduleSyncServiceClient>(options =>
            {
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

                    return configuration["AZURE_FUNCTIONS_ENVIRONMENT"] == "Development"
                        || configuration["ASPNETCORE_ENVIRONMENT"] == "Development";
                }
            }
        };
    }
}
