using InnoClinic.Contracts.Grpc;
using InnoClinic.Contracts.Grpc.Auth;
using InnoClinic.Messaging.Outbox;
using MassTransit;
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

            services.AddSingleton(TimeProvider.System);

            services.AddScoped<IPatientRepository, PatientRepository>();
            services.AddScoped<IMedicalStaffRepository, MedicalStaffRepository>();
            services.AddScoped<ISpecializationRepository, SpecializationRepository>();
            services.AddScoped<IOutboxRepository, OutboxRepository>();

            services.AddScoped<INotificationProducer, NotificationProducer>();

            var rabbitMqConnectionString = configuration.GetConnectionString(ConnectionConstants.RabbitMQConnection)
                ?? throw new InvalidOperationException("RabbitMQ connection string not found.");

            services.AddMassTransit(x =>
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(rabbitMqConnectionString);
                });
            });

            var appointmentsApiUrl = configuration[ConnectionConstants.AppointmentsApiUrl]
                ?? throw new InvalidOperationException($"{ConnectionConstants.AppointmentsApiUrl} not found in configuration.");

            services.AddInternalGrpcClient<StaffScheduleSyncService.StaffScheduleSyncServiceClient>(
                appointmentsApiUrl,
                configuration);

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
