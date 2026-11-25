using System.Net;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Serialization;
using Claims.Api.Application.Claims;
using Claims.Api.Domain;
using Claims.Api.Infrastructure.Messaging;
using Claims.Api.Infrastructure.Persistence;
using Claims.Api.Infrastructure.Persistence.Seeders;
using Confluent.Kafka;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.Kafka;
using Testcontainers.PostgreSql;

namespace Claims.Api.Tests;

/// <summary>
/// RFC-0002 section 4: a Kafka outage never fails or loses a REST call. Uses its own containers because the test
/// stops the broker, which would break the shared factory. The broker keeps its host port so the API reconnects.
/// </summary>
public sealed class BrokerOutageTests : IAsyncLifetime
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(90);

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly int _kafkaPort = FreePort();
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:18-alpine").Build();
    private KafkaContainer _kafka = null!;
    private string _bootstrap = null!;
    private OutageApiFactory _factory = null!;

    public async Task InitializeAsync()
    {
        _kafka = new KafkaBuilder("apache/kafka:4.1.0").WithPortBinding(_kafkaPort, 9092).Build();
        await Task.WhenAll(_postgres.StartAsync(), _kafka.StartAsync());
        _bootstrap = $"{_kafka.Hostname}:{_kafkaPort}";
        _factory = new OutageApiFactory(_postgres.GetConnectionString(), _bootstrap);
    }

    public async Task DisposeAsync()
    {
        await _factory.DisposeAsync();
        await _kafka.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task C3_Outbox_BrokerDown_RestSucceedsAndEventIsPublishedAfterRecovery()
    {
        using var client = _factory.CreateClient();
        await _kafka.StopAsync();

        var response = await client.PostAsJsonAsync(
            "/api/claims",
            new SubmitClaimRequest(DemoUsers.Alice, ClaimType.Motor, new DateOnly(2026, 11, 20), "description", 1m),
            Json);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var claim = (await response.Content.ReadFromJsonAsync<ClaimResponse>(Json))!;
        await Task.Delay(TimeSpan.FromSeconds(3));
        // The event waits in the outbox while the broker is down.
        Assert.Null(await PublishedAtAsync(claim.Id));

        await _kafka.StartAsync();

        await WaitUntilAsync(async () => await PublishedAtAsync(claim.Id) is not null);
        Assert.Equal("ClaimSubmitted", ReadFirstEventType(claim.Id));
    }

    private async Task<DateTimeOffset?> PublishedAtAsync(Guid claimId)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ClaimsDbContext>();
        return await db.OutboxMessages
            .Where(m => m.Key == claimId.ToString())
            .Select(m => m.PublishedAt)
            .SingleAsync();
    }

    private string ReadFirstEventType(Guid claimId)
    {
        using var consumer = new ConsumerBuilder<string, string>(new ConsumerConfig
        {
            BootstrapServers = _bootstrap,
            GroupId = $"test-{Guid.NewGuid():N}",
            AutoOffsetReset = AutoOffsetReset.Earliest,
        }).Build();
        consumer.Subscribe(Topics.ClaimEvents);

        var deadline = DateTime.UtcNow + Timeout;
        while (DateTime.UtcNow < deadline)
        {
            var result = consumer.Consume(TimeSpan.FromSeconds(1));
            if (result?.Message?.Key == claimId.ToString())
            {
                return EventEnvelope.Parse(result.Message.Value).EventType;
            }
        }

        throw new TimeoutException("The event never reached the topic.");
    }

    private static async Task WaitUntilAsync(Func<Task<bool>> condition)
    {
        var deadline = DateTime.UtcNow + Timeout;
        while (!await condition())
        {
            if (DateTime.UtcNow > deadline)
            {
                throw new TimeoutException("The outbox relay did not publish after the broker came back.");
            }

            await Task.Delay(500);
        }
    }

    private static int FreePort()
    {
        using var listener = new TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        return ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
    }

    private sealed class OutageApiFactory(string connectionString, string bootstrapServers) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:ClaimsDb", connectionString);
            builder.UseSetting("Kafka:BootstrapServers", bootstrapServers);
            builder.UseSetting("Kafka:OutboxPollIntervalMs", "500");
            builder.UseSetting("Database:MigrateOnStartup", "true");
        }
    }
}
