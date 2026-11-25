using Claims.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Claims.Api.Infrastructure.Messaging;

/// <summary>
/// Publishes outbox rows to Kafka, oldest first, and marks each one published after Kafka acknowledges it
/// (RFC-0002 section 4). A crash between the acknowledgement and the mark republishes the row, so delivery is at
/// least once and consumers dedupe on the event id. If Kafka is down the relay logs, waits and retries; REST calls
/// are not affected. Assumes one API instance.
/// </summary>
public sealed class OutboxRelay(
    IServiceScopeFactory scopeFactory,
    IEventPublisher publisher,
    KafkaTopicProvisioner topics,
    TimeProvider clock,
    IOptions<KafkaOptions> options,
    ILogger<OutboxRelay> logger) : BackgroundService
{
    private const int BatchSize = 100;
    private static readonly TimeSpan LagWarningThreshold = TimeSpan.FromSeconds(30);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromMilliseconds(options.Value.OutboxPollIntervalMs);
        using var timer = new PeriodicTimer(interval);

        do
        {
            try
            {
                await PublishPendingAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Outbox relay could not publish; will retry in {Interval}", interval);
            }
        }
        while (await WaitAsync(timer, stoppingToken));
    }

    /// <summary>Publishes one batch. Stops at the first failure so events keep their order.</summary>
    internal async Task PublishPendingAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ClaimsDbContext>();

        var pending = await db.OutboxMessages
            .AsNoTracking()
            .Where(m => m.PublishedAt == null)
            .OrderBy(m => m.OccurredAt).ThenBy(m => m.Id)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        if (pending.Count == 0)
        {
            return;
        }

        // Lag is how long the oldest unpublished event has waited (RFC-0002 section 9).
        var lag = clock.GetUtcNow() - pending[0].OccurredAt;
        if (lag > LagWarningThreshold)
        {
            logger.LogWarning("Outbox lag is {Lag}: {Count} events waiting", lag, pending.Count);
        }
        else
        {
            logger.LogDebug("Outbox lag is {Lag}: {Count} events waiting", lag, pending.Count);
        }

        await topics.EnsureCreatedAsync(cancellationToken);

        foreach (var message in pending)
        {
            await publisher.PublishRawAsync(
                message.Topic,
                message.Key,
                message.Payload,
                new Dictionary<string, string> { ["eventType"] = message.EventType },
                cancellationToken);

            var publishedAt = clock.GetUtcNow();
            await db.OutboxMessages
                .Where(m => m.Id == message.Id)
                .ExecuteUpdateAsync(s => s.SetProperty(m => m.PublishedAt, publishedAt), cancellationToken);
        }

        logger.LogDebug("Outbox relay published {Count} events", pending.Count);
    }

    private static async Task<bool> WaitAsync(PeriodicTimer timer, CancellationToken cancellationToken)
    {
        try
        {
            return await timer.WaitForNextTickAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }
}
