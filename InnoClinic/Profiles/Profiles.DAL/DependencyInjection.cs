using InnoClinic.Shared.Protos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Profiles.DAL.Data;
using Profiles.DAL.Interfaces;
using Profiles.DAL.Repositories;
using Profiles.Domain.Constants;

namespace Profiles.DAL;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddDataAccess(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString(ConnectionConstants.DefaultConnection)
                ?? throw new InvalidOperationException("Connection string not found.");

            services.AddDbContext<ProfilesDbContext>(options =>
                options.UseNpgsql(connectionString));

            services.AddScoped<IPatientRepository, PatientRepository>();
            services.AddScoped<IMedicalStaffRepository, MedicalStaffRepository>();
            services.AddScoped<ISpecializationRepository, SpecializationRepository>();
            services.AddScoped<IOutboxRepository, OutboxRepository>();

            services.AddGrpcClient<StaffScheduleSyncService.StaffScheduleSyncServiceClient>(options =>
            {
                var appointmentsApiUrl = configuration[ConnectionConstants.AppointmentsApiUrl] 
                    ?? throw new ArgumentNullException($"{ConnectionConstants.AppointmentsApiUrl} not found in configuration.");

                options.Address = new Uri(appointmentsApiUrl);
            })
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                var handler = new SocketsHttpHandler
                {
                    EnableMultipleHttp2Connections = true,
                    PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
                    KeepAlivePingDelay = TimeSpan.FromSeconds(60),
                    KeepAlivePingTimeout = TimeSpan.FromSeconds(30),
                    SslOptions = new System.Net.Security.SslClientAuthenticationOptions
                    {
                        RemoteCertificateValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
                    }
                };
                return handler;
            });

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
