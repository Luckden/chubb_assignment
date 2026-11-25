using System.Text.Json;
using Claims.Api.Infrastructure.Messaging;
using Claims.Api.Tests.Infrastructure;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;

namespace Claims.Api.Tests;

[Collection(ApiCollection.Name)]
public class MessagingTests(ClaimsApiFactory factory)
{
    private sealed record PingEvent(Guid Id, string Message);

    [Fact]
    public async Task C3_Publish_Event_CanBeConsumedFromKafka()
    {
        var topic = $"test-{Guid.NewGuid():N}";
        var sent = new PingEvent(Guid.NewGuid(), "hello");
        var publisher = factory.Services.GetRequiredService<IEventPublisher>();

        await publisher.PublishAsync(topic, sent.Id.ToString(), sent);

        using var consumer = new ConsumerBuilder<string, string>(new ConsumerConfig
        {
            BootstrapServers = factory.KafkaBootstrapServers,
            GroupId = $"test-{Guid.NewGuid():N}",
            AutoOffsetReset = AutoOffsetReset.Earliest,
        }).Build();
        consumer.Subscribe(topic);

        var result = consumer.Consume(TimeSpan.FromSeconds(30));

        Assert.NotNull(result);
        Assert.Equal(sent.Id.ToString(), result.Message.Key);
        Assert.Equal(sent, JsonSerializer.Deserialize<PingEvent>(result.Message.Value, JsonSerializerOptions.Web));
    }
}
