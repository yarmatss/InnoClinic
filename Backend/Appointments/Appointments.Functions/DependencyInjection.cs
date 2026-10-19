using Appointments.Infrastructure.Data;
using Appointments.Infrastructure.Services;
using InnoClinic.Contracts.Grpc;
using InnoClinic.Contracts.Grpc.Auth;
using InnoClinic.Messaging.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

            services.AddInternalGrpcClient<PatientService.PatientServiceClient>(profilesApiUrl, configuration);
            services.AddInternalGrpcClient<StaffScheduleSyncService.StaffScheduleSyncServiceClient>(profilesApiUrl, configuration);

            return services;
        }
    }
}
