using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace Claims.Api.Infrastructure.Messaging;

/// <summary>
/// Background consumer skeleton. Subscribes to the configured topics and logs messages.
/// TODO: dispatch messages to handlers once integration events are defined.
/// </summary>
public sealed class KafkaConsumerService(IOptions<KafkaOptions> options, ILogger<KafkaConsumerService> logger) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken) =>
        // Confluent's Consume() is blocking, so run the loop on a dedicated thread.
        Task.Factory.StartNew(() => ConsumeLoop(stoppingToken), stoppingToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);

    private void ConsumeLoop(CancellationToken stoppingToken)
    {
        var settings = options.Value;
        if (settings.SubscribeTopics.Length == 0)
        {
            logger.LogInformation("Kafka consumer has no topics configured; not starting.");
            return;
        }

        var config = new ConsumerConfig
        {
            BootstrapServers = settings.BootstrapServers,
            GroupId = settings.ConsumerGroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe(settings.SubscribeTopics);
        logger.LogInformation("Kafka consumer subscribed to {Topics}", string.Join(", ", settings.SubscribeTopics));

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var result = consumer.Consume(stoppingToken);
                logger.LogInformation("Received message on {Topic} key={Key}", result.Topic, result.Message.Key);
                // TODO: route to a handler, then commit only after successful handling.
                consumer.Commit(result);
            }
        }
        catch (OperationCanceledException)
        {
            // shutting down
        }
        finally
        {
            consumer.Close();
        }
    }
}
