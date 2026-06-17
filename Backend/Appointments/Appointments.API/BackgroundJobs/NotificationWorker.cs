using InnoClinic.Messaging.Outbox;
using Microsoft.Extensions.Options;
using Polly.Registry;

namespace Appointments.API.BackgroundJobs;

public class NotificationWorker(
    IConfiguration configuration,
    IServiceScopeFactory scopeFactory,
    IOptionsMonitor<OutboxOptions> options,
    ILogger<NotificationWorker> logger,
    ResiliencePipelineProvider<string> pipelineProvider) 
    : PostgresOutboxProcessor(
        configuration.GetConnectionString("DefaultConnection")!, 
        scopeFactory,
        options,
        logger,
        pipelineProvider)
{
}
