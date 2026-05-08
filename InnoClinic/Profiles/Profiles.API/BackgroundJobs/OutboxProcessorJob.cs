using Google.Protobuf;
using Grpc.Core;
using InnoClinic.Shared.Protos;
using Profiles.DAL.Interfaces;

namespace Profiles.API.BackgroundJobs;

public class OutboxProcessorJob(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<OutboxProcessorJob> logger) : BackgroundService
{
    private readonly int _intervalInSeconds = configuration.GetValue("Outbox:IntervalInSeconds", 5);
    private readonly int _batchSize = configuration.GetValue("Outbox:BatchSize", 20);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Outbox Processor Background Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "A fatal error occurred while processing the outbox.");
            }

            await Task.Delay(TimeSpan.FromSeconds(_intervalInSeconds), stoppingToken);
        }
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken stoppingToken)
    {
        using var scope = scopeFactory.CreateScope();

        var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
        var grpcClient = scope.ServiceProvider.GetRequiredService<StaffScheduleSyncService.StaffScheduleSyncServiceClient>();

        var messages = await outboxRepository.GetUnprocessedMessagesAsync(_batchSize, stoppingToken);

        if (messages.Count == 0) 
            return;

        foreach (var message in messages)
        {
            try
            {
                var request = JsonParser.Default.Parse<SyncStaffProfileRequest>(message.Content);

                var response = await grpcClient.SyncStaffProfileAsync(request, cancellationToken: stoppingToken);

                if (response.Success)
                {
                    message.ProcessedOnUtc = DateTime.UtcNow;
                    logger.LogInformation("Successfully synced Staff ID: {StaffId}", request.MedicalStaffId);
                }
            }
            catch (RpcException rpcEx)
            {
                logger.LogWarning(rpcEx, "gRPC network error for Outbox Message {Id}. Retrying later.", message.Id);
                message.Error = $"gRPC Error: {rpcEx.StatusCode}";
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to process Outbox Message {Id}", message.Id);
                message.Error = ex.Message;
            }

            outboxRepository.MarkUpdate(message);
        }

        await outboxRepository.SaveChangesAsync(stoppingToken);
    }
}
