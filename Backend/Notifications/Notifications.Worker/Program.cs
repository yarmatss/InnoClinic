using MassTransit;
using Notifications.Domain.Constants;

var builder = Host.CreateApplicationBuilder(args);

var rabbitMqConnectionString = builder.Configuration.GetConnectionString(ConnectionConstants.RabbitMQ) 
    ?? throw new InvalidOperationException($"Connection string '{ConnectionConstants.RabbitMQ}' not found.");

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbitMqConnectionString);
        cfg.ConfigureEndpoints(context);
    });
});

var host = builder.Build();
host.Run();
