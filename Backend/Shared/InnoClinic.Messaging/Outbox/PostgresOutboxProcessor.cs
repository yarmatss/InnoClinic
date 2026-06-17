using Dapper;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Text.Json;
using Polly;
using Polly.Registry;
using Polly.Timeout;
using Polly.Retry;
using InnoClinic.Messaging.Constants;
using System.Net.Sockets;

namespace InnoClinic.Messaging.Outbox;

public abstract class PostgresOutboxProcessor(
    string connectionString,
    IServiceScopeFactory scopeFactory,
    IOptionsMonitor<OutboxOptions> options,
    ILogger logger,
    ResiliencePipelineProvider<string> pipelineProvider) : BackgroundService
{
    private OutboxOptions Config => options.CurrentValue;
    private ResiliencePipeline? _pipeline;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _pipeline = pipelineProvider.TryGetPipeline(MessagingConstants.OutboxResiliencePipeline, out var pipeline)
            ? pipeline
            : CreateSafeDefaultPipeline();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Fatal error in Outbox Processor");
            }

            await Task.Delay(TimeSpan.FromSeconds(Config.IntervalInSeconds), stoppingToken);
        }
    }

    private static ResiliencePipeline CreateSafeDefaultPipeline()
    {
        return new ResiliencePipelineBuilder()
            .AddTimeout(TimeSpan.FromSeconds(5))
            .AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<Exception>(),
                MaxRetryAttempts = 1,
                Delay = TimeSpan.FromSeconds(1)
            })
            .Build();
    }

    private async Task ProcessMessagesAsync(CancellationToken stoppingToken)
    {
        using var scope = scopeFactory.CreateScope();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        var sanitizedTableName = Config.TableName.Replace("\"", "").Replace("'", "").Replace(";", "");

        using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(stoppingToken);

        var selectSql = $@"
            SELECT * FROM ""{sanitizedTableName}""
            WHERE ""Status"" = @PendingStatus AND ""RetryCount"" < @MaxRetry
            ORDER BY ""CreatedAtUtc""
            FOR UPDATE SKIP LOCKED
            LIMIT @BatchSize";

        var command = new CommandDefinition(selectSql, new
        {
            PendingStatus = OutboxStatus.Pending.ToString(),
            MaxRetry = Config.MaxRetryCount,
            Config.BatchSize
        }, cancellationToken: stoppingToken);

        var messages = (await connection.QueryAsync<NotificationOutbox>(command)).ToList();

        if (messages.Count == 0) 
            return;

        foreach (var message in messages)
        {
            try
            {
                var type = Type.GetType(message.MessageType)
                    ?? throw new InvalidOperationException($"Could not resolve message type: {message.MessageType}");
                var payload = JsonSerializer.Deserialize(message.Payload, type);
                
                await _pipeline!.ExecuteAsync(async ct => 
                {
                    await publishEndpoint.Publish(payload!, type, context =>
                    {
                        context.Headers.Set("MessageType", message.MessageType);
                    }, ct);
                }, stoppingToken);

                var updateCommand = new CommandDefinition($@"
                    UPDATE ""{sanitizedTableName}""
                    SET ""Status"" = @Status, ""ProcessedAtUtc"" = @Now
                    WHERE ""Id"" = @Id", new 
                    { 
                        Status = OutboxStatus.Processed.ToString(), 
                        Now = DateTime.UtcNow, 
                        message.Id 
                    },
                    cancellationToken: stoppingToken);

                await connection.ExecuteAsync(updateCommand);
            }
            catch (Exception ex)
            {
                var isInfrastructureError = IsInfrastructureError(ex);
                
                if (isInfrastructureError)
                {
                    logger.LogWarning("Transient infrastructure error while publishing message {Id}. Will retry on next poll. Error: {Message}", 
                        message.Id, ex.Message);
                }
                else
                {
                    logger.LogWarning(ex, "Permanent or data-related error while publishing message {Id}", message.Id);
                }

                var retryIncrement = isInfrastructureError ? 0 : 1;

                var errorCommand = new CommandDefinition($@"
                    UPDATE ""{sanitizedTableName}""
                    SET ""RetryCount"" = ""RetryCount"" + @Increment, 
                        ""ErrorMessage"" = @Error,
                        ""Status"" = CASE 
                            WHEN ""RetryCount"" + @Increment >= @MaxRetry THEN @FailedStatus 
                            ELSE @PendingStatus 
                        END
                    WHERE ""Id"" = @Id",
                    new 
                    {
                        message.Id,
                        Increment = retryIncrement,
                        Error = ex.Message, 
                        MaxRetry = Config.MaxRetryCount,
                        FailedStatus = OutboxStatus.Failed.ToString(),
                        PendingStatus = OutboxStatus.Pending.ToString()
                    },
                    cancellationToken: stoppingToken);

                await connection.ExecuteAsync(errorCommand);
            }
        }
    }

    private static bool IsInfrastructureError(Exception ex)
    {
        return ex is TimeoutRejectedException || 
               ex is OperationCanceledException || 
               ex is SocketException || 
               ex.GetType().Name.Contains("BrokerUnreachableException") || 
               ex.GetType().Name.Contains("RabbitMqConnectionException") || 
               (ex.InnerException != null && IsInfrastructureError(ex.InnerException));
    }
}
