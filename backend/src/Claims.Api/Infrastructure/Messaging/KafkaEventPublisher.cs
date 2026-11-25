using System.Text;
using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace Claims.Api.Infrastructure.Messaging;

public sealed class KafkaEventPublisher : IEventPublisher, IDisposable
{
    // Created on first use so that building the host (OpenAPI export, tests) never opens a broker connection.
    private readonly Lazy<IProducer<string, string>> _producer;
    private readonly ILogger<KafkaEventPublisher> _logger;

    public KafkaEventPublisher(IOptions<KafkaOptions> options, ILogger<KafkaEventPublisher> logger)
    {
        _logger = logger;
        var config = new ProducerConfig
        {
            BootstrapServers = options.Value.BootstrapServers,
            ClientId = options.Value.ClientId,
            Acks = Acks.All,
            EnableIdempotence = true,

            // A dead broker must fail a publish in seconds, not the 5-minute default, so the relay can retry (RFC-0002).
            MessageTimeoutMs = 10_000,
        };
        _producer = new Lazy<IProducer<string, string>>(() => new ProducerBuilder<string, string>(config).Build());
    }

    public Task PublishAsync<TEvent>(string topic, string key, TEvent @event, CancellationToken cancellationToken = default) =>
        PublishRawAsync(topic, key, JsonSerializer.Serialize(@event, JsonSerializerOptions.Web), null, cancellationToken);

    public async Task PublishRawAsync(
        string topic,
        string key,
        string payload,
        IReadOnlyDictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        var message = new Message<string, string> { Key = key, Value = payload };
        if (headers is { Count: > 0 })
        {
            message.Headers = [];
            foreach (var (name, value) in headers)
            {
                message.Headers.Add(name, Encoding.UTF8.GetBytes(value));
            }
        }

        var result = await _producer.Value.ProduceAsync(topic, message, cancellationToken);
        _logger.LogDebug("Published to {TopicPartitionOffset}", result.TopicPartitionOffset);
    }

    public void Dispose()
    {
        if (_producer.IsValueCreated)
        {
            _producer.Value.Flush(TimeSpan.FromSeconds(5));
            _producer.Value.Dispose();
        }
    }
}
