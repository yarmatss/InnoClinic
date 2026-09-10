using MassTransit;
using Notifications.Worker.Consumers;
using Notifications.Worker.Constants;

namespace Notifications.Worker.Extensions;

public static class MessagingExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddMessaging(IConfiguration configuration)
        {
            var rabbitMqConnectionString = configuration.GetConnectionString(ConnectionConstants.RabbitMQ)
                ?? throw new InvalidOperationException($"Connection string '{ConnectionConstants.RabbitMQ}' not found.");

            services.AddMassTransit(x =>
            {
                x.AddConsumer<AppointmentBookedConsumer>();
                x.AddConsumer<AppointmentReminderConsumer>();
                x.AddConsumer<PatientCreatedConsumer>();

                x.AddConfigureEndpointsCallback((context, name, cfg) =>
                {
                    cfg.RethrowFaultedMessages();
                });

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(rabbitMqConnectionString);
                    cfg.ConfigureEndpoints(context);
                });
            });

            return services;
        }
    }
}
