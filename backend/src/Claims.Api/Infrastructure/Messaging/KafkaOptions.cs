namespace Claims.Api.Infrastructure.Messaging;

public sealed class KafkaOptions
{
    public const string SectionName = "Kafka";

    public string BootstrapServers { get; init; } = "localhost:9092";
    public string ClientId { get; init; } = "claims-api";
    public string ConsumerGroupId { get; init; } = "claims-api";

    /// <summary>When false the background consumer is not registered (running without Kafka).</summary>
    public bool ConsumerEnabled { get; init; }

    /// <summary>How often the outbox relay looks for unpublished events.</summary>
    public int OutboxPollIntervalMs { get; init; } = 1000;

    /// <summary>Published outbox rows older than this many days are deleted. Unpublished rows are never deleted.</summary>
    public int OutboxRetentionDays { get; init; } = 7;

    /// <summary>Attempts the consumer makes on one message before it goes to the dead-letter topic.</summary>
    public int ConsumerMaxAttempts { get; init; } = 3;

    /// <summary>Wait before retry n is this times n.</summary>
    public int ConsumerRetryBackoffMs { get; init; } = 250;
}
