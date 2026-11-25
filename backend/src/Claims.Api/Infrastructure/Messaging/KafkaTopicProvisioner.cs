using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Options;

namespace Claims.Api.Infrastructure.Messaging;

/// <summary>
/// Creates the event topic and its dead-letter topic (3 partitions, replication factor 1 on the single-node broker).
/// Auto-create is off in Compose, so a typo in a topic name fails loudly. The API must start without Kafka, so
/// callers (relay and consumer) ask for the topics before they use them and retry until the broker answers.
/// </summary>
public sealed class KafkaTopicProvisioner(IOptions<KafkaOptions> options, ILogger<KafkaTopicProvisioner> logger)
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private bool _created;

    public async Task EnsureCreatedAsync(CancellationToken cancellationToken)
    {
        if (_created)
        {
            return;
        }

        await _gate.WaitAsync(cancellationToken);
        try
        {
            if (_created)
            {
                return;
            }

            using var admin = new AdminClientBuilder(new AdminClientConfig
            {
                BootstrapServers = options.Value.BootstrapServers,
                SocketTimeoutMs = 10_000,
            }).Build();

            try
            {
                await admin.CreateTopicsAsync(
                    Topics.All.Select(name => new TopicSpecification { Name = name, NumPartitions = 3, ReplicationFactor = 1 }),
                    new CreateTopicsOptions { RequestTimeout = TimeSpan.FromSeconds(10) });
                logger.LogInformation("Created Kafka topics {Topics}", string.Join(", ", Topics.All));
            }
            catch (CreateTopicsException ex) when (ex.Results.All(r => r.Error.Code is ErrorCode.TopicAlreadyExists or ErrorCode.NoError))
            {
                // Created by an earlier run.
            }

            _created = true;
        }
        finally
        {
            _gate.Release();
        }
    }
}
