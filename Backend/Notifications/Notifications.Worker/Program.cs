using Notifications.Worker.Extensions;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddMessaging(builder.Configuration);

var host = builder.Build();
host.Run();
