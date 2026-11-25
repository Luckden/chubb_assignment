using System.Text;
using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace Claims.Api.Infrastructure.Messaging;

/// <summary>
/// Reads <see cref="Topics.ClaimEvents"/> and hands each event to the <see cref="IEventHandler"/>s that want it
/// (RFC-0002 sections 5 and 8). The offset is committed only after the handlers finish. A message that still fails after
/// the configured attempts goes to the dead-letter topic with the error in a header, and then its offset is committed so
/// one poison message cannot block the partition. Unexpected errors are logged and the loop keeps running.
/// </summary>
public sealed class KafkaConsumerService(
    IOptions<KafkaOptions> options,
    IServiceScopeFactory scopeFactory,
    IEventPublisher publisher,
    KafkaTopicProvisioner topics,
    ILogger<KafkaConsumerService> logger) : BackgroundService
{
    private static readonly TimeSpan PollTimeout = TimeSpan.FromMilliseconds(500);
    private static readonly TimeSpan RecoveryDelay = TimeSpan.FromSeconds(2);

    // Confluent's Consume() blocks, so keep it off the host's startup path.
    protected override Task ExecuteAsync(CancellationToken stoppingToken) =>
        Task.Run(() => RunAsync(stoppingToken), stoppingToken);

    private async Task RunAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await topics.EnsureCreatedAsync(stoppingToken);
                await ConsumeAsync(settings, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Kafka consumer stopped unexpectedly; restarting in {Delay}", RecoveryDelay);
                await Task.Delay(RecoveryDelay, stoppingToken).ContinueWith(_ => { }, CancellationToken.None);
            }
        }
    }

    private async Task ConsumeAsync(KafkaOptions settings, CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = settings.BootstrapServers,
            GroupId = settings.ConsumerGroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe(Topics.ClaimEvents);
        logger.LogInformation("Kafka consumer subscribed to {Topic}", Topics.ClaimEvents);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                ConsumeResult<string, string>? result;
                try
                {
                    result = consumer.Consume(PollTimeout);
                }
                catch (ConsumeException ex)
                {
                    logger.LogWarning(ex, "Kafka consume error: {Reason}", ex.Error.Reason);
                    await Task.Delay(RecoveryDelay, stoppingToken);
                    continue;
                }

                if (result?.Message is null)
                {
                    continue;
                }

                await ProcessWithRetriesAsync(result, settings, stoppingToken);
                consumer.Commit(result);
            }
        }
        finally
        {
            consumer.Close();
        }
    }

    private async Task ProcessWithRetriesAsync(ConsumeResult<string, string> result, KafkaOptions settings, CancellationToken ct)
    {
        Exception? last = null;
        for (var attempt = 1; attempt <= settings.ConsumerMaxAttempts; attempt++)
        {
            try
            {
                await DispatchAsync(result.Message.Value, ct);
                return;
            }
            catch (Exception ex) when (!ct.IsCancellationRequested)
            {
                last = ex;
                logger.LogWarning(ex, "Handling {Topic}[{Partition}]@{Offset} failed (attempt {Attempt}/{Max})",
                    result.Topic, result.Partition.Value, result.Offset.Value, attempt, settings.ConsumerMaxAttempts);
                if (attempt < settings.ConsumerMaxAttempts)
                {
                    await Task.Delay(settings.ConsumerRetryBackoffMs * attempt, ct);
                }
            }
        }

        await DeadLetterAsync(result, last!, ct);
    }

    private async Task DispatchAsync(string payload, CancellationToken ct)
    {
        var envelope = EventEnvelope.Parse(payload);

        await using var scope = scopeFactory.CreateAsyncScope();
        foreach (var handler in scope.ServiceProvider.GetServices<IEventHandler>().Where(h => h.CanHandle(envelope.EventType)))
        {
            await handler.HandleAsync(envelope, ct);
        }
    }

    private async Task DeadLetterAsync(ConsumeResult<string, string> result, Exception error, CancellationToken ct)
    {
        logger.LogError(error, "Sending {Topic}[{Partition}]@{Offset} to the dead-letter topic",
            result.Topic, result.Partition.Value, result.Offset.Value);

        var headers = new Dictionary<string, string>
        {
            ["error"] = error.Message,
            ["originalTopic"] = result.Topic,
        };
        if (result.Message.Headers?.TryGetLastBytes("eventType", out var eventType) == true)
        {
            headers["eventType"] = Encoding.UTF8.GetString(eventType);
        }

        // If this fails the exception escapes before the commit, so the message is tried again after the restart delay.
        await publisher.PublishRawAsync(Topics.ClaimEventsDeadLetter, result.Message.Key ?? string.Empty, result.Message.Value, headers, ct);
    }
}
