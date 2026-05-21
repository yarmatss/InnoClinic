using Google.Protobuf;
using Grpc.Core;
using InnoClinic.Contracts.Grpc;
using Microsoft.Extensions.Options;
using Profiles.API.Extensions;
using Profiles.API.Options;
using Profiles.DAL.Interfaces;

namespace Profiles.API.BackgroundJobs;

public class OutboxProcessorJob(
    IServiceScopeFactory scopeFactory,
    IOptionsMonitor<OutboxOptions> optionsMonitor,
    TimeProvider timeProvider,
    ILogger<OutboxProcessorJob> logger) : BackgroundService
{
    private int IntervalInSeconds => optionsMonitor.CurrentValue.IntervalInSeconds;
    private int BatchSize => optionsMonitor.CurrentValue.BatchSize;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogOutboxProcessorStarting();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogOutboxFatalError(ex);
            }

            await Task.Delay(TimeSpan.FromSeconds(IntervalInSeconds), timeProvider, stoppingToken);
        }
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken stoppingToken)
    {
        using var scope = scopeFactory.CreateScope();

        var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
        var grpcClient = scope.ServiceProvider.GetRequiredService<StaffScheduleSyncService.StaffScheduleSyncServiceClient>();

        var messages = await outboxRepository.GetUnprocessedMessagesAsync(BatchSize, stoppingToken);

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
                    message.ProcessedOnUtc = timeProvider.GetUtcNow().UtcDateTime;
                    logger.LogOutboxSyncSuccess(request.MedicalStaffId);
                }
            }
            catch (RpcException rpcEx)
            {
                logger.LogOutboxGrpcError(rpcEx, message.Id);
                message.Error = $"gRPC Error: {rpcEx.StatusCode}";
            }
            catch (Exception ex)
            {
                logger.LogOutboxProcessingError(ex, message.Id);
                message.Error = ex.Message;
            }

            outboxRepository.MarkUpdate(message);
        }

        await outboxRepository.SaveChangesAsync(stoppingToken);
    }
}
