namespace Claims.Api.Infrastructure.Messaging;

public sealed class KafkaOptions
{
    public const string SectionName = "Kafka";

    public string BootstrapServers { get; init; } = "localhost:9092";
    public string ClientId { get; init; } = "claims-api";
    public string ConsumerGroupId { get; init; } = "claims-api";

    /// <summary>When false the background consumer is not registered (tests / running without Kafka).</summary>
    public bool ConsumerEnabled { get; init; }

    public string[] SubscribeTopics { get; init; } = [];
}
