using Appointments.Functions;
using Appointments.Functions.Options;
using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING")))
{
    builder.Services.AddOpenTelemetry()
        .UseFunctionsWorkerDefaults()
        .UseAzureMonitorExporter();
}

builder.Services.Configure<ReminderOptions>(
    builder.Configuration.GetSection(ReminderOptions.SectionName));

builder.Services
    .AddDatabase(builder.Configuration)
    .AddOutbox()
    .AddProfilesGrpcClients(builder.Configuration);
await builder.Build().RunAsync();
