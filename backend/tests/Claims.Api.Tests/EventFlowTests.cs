using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Claims.Api.Application.Claims;
using Claims.Api.Application.Notifications;
using Claims.Api.Domain;
using Claims.Api.Infrastructure.Messaging;
using Claims.Api.Infrastructure.Persistence;
using Claims.Api.Infrastructure.Persistence.Seeders;
using Claims.Api.Tests.Infrastructure;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Claims.Api.Tests;

/// <summary>
/// RFC-0002 end to end: REST call, outbox, Kafka topic, consumer, notification. Requires Docker.
/// C3_ tests trace to constraint C3 (use Kafka); R3_ and R4_ tests trace to the requirements the notifications serve.
/// </summary>
[Collection(ApiCollection.Name)]
public class EventFlowTests(ClaimsApiFactory factory)
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(45);

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly HttpClient _client = factory.CreateClient();

    // --- Arrange and act helpers ---------------------------------------------------------------

    private async Task<T> PostAsync<T>(string path, object body, HttpStatusCode expected = HttpStatusCode.OK)
    {
        var response = await _client.PostAsJsonAsync(path, body, Json);
        Assert.Equal(expected, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<T>(Json))!;
    }

    private async Task<ClaimResponse> SubmittedAsync() =>
        await PostAsync<ClaimResponse>(
            "/api/claims",
            new SubmitClaimRequest(DemoUsers.Alice, ClaimType.Motor, new DateOnly(2026, 11, 20), "description", 1m),
            HttpStatusCode.Created);

    private async Task<ClaimResponse> UnderReviewAsync()
    {
        var claim = await SubmittedAsync();
        return await PostAsync<ClaimResponse>($"/api/claims/{claim.Id}/pick-up", new PickUpClaimRequest(DemoUsers.Carol));
    }

    private async Task<ClaimResponse> InfoRequestedAsync()
    {
        var claim = await UnderReviewAsync();
        return await PostAsync<ClaimResponse>(
            $"/api/claims/{claim.Id}/information-requests", new RequestInformationRequest(DemoUsers.Carol, "question"));
    }

    private Task<ClaimResponse> SettleAsync(Guid claimId) =>
        PostAsync<ClaimResponse>($"/api/claims/{claimId}/settle", new SettleClaimRequest(DemoUsers.Carol, "reason"));

    private async Task<List<NotificationResponse>> NotificationsAsync(Guid userId, Guid claimId) =>
        (await _client.GetFromJsonAsync<List<NotificationResponse>>($"/api/notifications?userId={userId}", Json))!
            .Where(n => n.ClaimId == claimId)
            .ToList();

    private async Task<List<NotificationResponse>> NotificationsEventuallyAsync(Guid userId, Guid claimId, int count)
    {
        var deadline = DateTime.UtcNow + Timeout;
        while (true)
        {
            var found = await NotificationsAsync(userId, claimId);
            if (found.Count >= count || DateTime.UtcNow > deadline)
            {
                return found;
            }

            await Task.Delay(250);
        }
    }

    private async Task<string> OutboxPayloadAsync(Guid claimId, string eventType)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ClaimsDbContext>();
        return await db.OutboxMessages
            .Where(m => m.Key == claimId.ToString() && m.EventType == eventType)
            .Select(m => m.Payload)
            .SingleAsync();
    }

    private async Task ProduceAsync(string topic, string key, string value)
    {
        using var producer = new ProducerBuilder<string, string>(
            new ProducerConfig { BootstrapServers = factory.KafkaBootstrapServers }).Build();
        await producer.ProduceAsync(topic, new Message<string, string> { Key = key, Value = value });
    }

    /// <summary>Reads a topic from the start until <paramref name="isDone"/> says the collected messages are enough.</summary>
    private List<ConsumeResult<string, string>> Consume(
        string topic, Func<ConsumeResult<string, string>, bool> keep, Func<List<ConsumeResult<string, string>>, bool> isDone)
    {
        using var consumer = new ConsumerBuilder<string, string>(new ConsumerConfig
        {
            BootstrapServers = factory.KafkaBootstrapServers,
            GroupId = $"test-{Guid.NewGuid():N}",
            AutoOffsetReset = AutoOffsetReset.Earliest,
        }).Build();
        consumer.Subscribe(topic);

        var collected = new List<ConsumeResult<string, string>>();
        var deadline = DateTime.UtcNow + Timeout;
        while (DateTime.UtcNow < deadline && !isDone(collected))
        {
            var result = consumer.Consume(TimeSpan.FromSeconds(1));
            if (result?.Message is not null && keep(result))
            {
                collected.Add(result);
            }
        }

        return collected;
    }

    // --- Outbox and topic ----------------------------------------------------------------------

    [Fact]
    public async Task C3_Outbox_ClaimLifecycle_PublishesEventsInOrderKeyedByClaim()
    {
        var claim = await InfoRequestedAsync();

        var messages = Consume(
            Topics.ClaimEvents,
            r => r.Message.Key == claim.Id.ToString(),
            collected => collected.Count >= 3);

        var types = messages.Select(m => EventEnvelope.Parse(m.Message.Value).EventType).ToList();
        Assert.Equal(["ClaimSubmitted", "ClaimPickedUp", "InformationRequested"], types);
        Assert.All(messages, m => Assert.Equal(messages[0].Partition, m.Partition));
    }

    [Fact]
    public async Task C3_Outbox_PublishedEvent_CarriesEnvelopeAndEventTypeHeader()
    {
        var claim = await SubmittedAsync();

        var message = Consume(Topics.ClaimEvents, r => r.Message.Key == claim.Id.ToString(), c => c.Count >= 1).Single();

        var envelope = EventEnvelope.Parse(message.Message.Value);
        Assert.Equal(claim.Id, envelope.ClaimId);
        Assert.Equal(1, envelope.SchemaVersion);
        Assert.Equal(claim.ClaimNumber, envelope.Data.GetProperty("claimNumber").GetString());
        Assert.Equal("ClaimSubmitted", Encoding.UTF8.GetString(message.Message.Headers.GetLastBytes("eventType")));
    }

    // --- Notifications -------------------------------------------------------------------------

    [Fact]
    public async Task R3_Notifications_InformationRequested_ClaimantIsNotified()
    {
        var claim = await InfoRequestedAsync();

        var notifications = await NotificationsEventuallyAsync(DemoUsers.Alice, claim.Id, 1);

        var notification = Assert.Single(notifications);
        Assert.Contains("more information", notification.Message);
        Assert.Null(notification.ReadAt);
    }

    [Fact]
    public async Task R4_Notifications_ClaimSettled_ClaimantIsNotified()
    {
        var claim = await UnderReviewAsync();

        await SettleAsync(claim.Id);

        var notification = Assert.Single(await NotificationsEventuallyAsync(DemoUsers.Alice, claim.Id, 1));
        Assert.Contains("decision", notification.Message);
    }

    [Fact]
    public async Task R3_Notifications_OtherUser_DoesNotSeeTheNotification()
    {
        var claim = await InfoRequestedAsync();
        await NotificationsEventuallyAsync(DemoUsers.Alice, claim.Id, 1);

        var others = await NotificationsAsync(DemoUsers.Carol, claim.Id);

        Assert.Empty(others);
    }

    [Fact]
    public async Task R3_Notifications_MarkRead_SetsReadAt()
    {
        var claim = await InfoRequestedAsync();
        var notification = Assert.Single(await NotificationsEventuallyAsync(DemoUsers.Alice, claim.Id, 1));

        var read = await PostAsync<NotificationResponse>(
            $"/api/notifications/{notification.Id}/read", new MarkNotificationReadRequest(DemoUsers.Alice));

        Assert.NotNull(read.ReadAt);
    }

    [Fact]
    public async Task R3_Notifications_MarkReadByAnotherUser_ReturnsForbidden()
    {
        var claim = await InfoRequestedAsync();
        var notification = Assert.Single(await NotificationsEventuallyAsync(DemoUsers.Alice, claim.Id, 1));

        var response = await _client.PostAsJsonAsync(
            $"/api/notifications/{notification.Id}/read", new MarkNotificationReadRequest(DemoUsers.Carol), Json);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task R3_Notifications_ListWithoutUser_ReturnsValidationProblem()
    {
        var response = await _client.GetAsync("/api/notifications");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // --- Delivery semantics --------------------------------------------------------------------

    [Fact]
    public async Task C3_Consumer_DuplicateEvent_CreatesOneNotification()
    {
        var claim = await InfoRequestedAsync();
        await NotificationsEventuallyAsync(DemoUsers.Alice, claim.Id, 1);
        var original = await OutboxPayloadAsync(claim.Id, "InformationRequested");

        // The same event again (at-least-once delivery), then a later event on the same key as a marker: the
        // consumer has handled the duplicate once the marker's notification appears.
        await ProduceAsync(Topics.ClaimEvents, claim.Id.ToString(), original);
        await PostAsync<ClaimResponse>(
            $"/api/claims/{claim.Id}/information-response", new ProvideInformationRequest(DemoUsers.Alice, "answer"));
        await SettleAsync(claim.Id);
        var notifications = await NotificationsEventuallyAsync(DemoUsers.Alice, claim.Id, 2);

        Assert.Equal(2, notifications.Count);
    }

    [Fact]
    public async Task C3_Consumer_PoisonMessage_GoesToDeadLetterTopicAndConsumerKeepsRunning()
    {
        var poisonKey = $"poison-{Guid.NewGuid():N}";
        await ProduceAsync(Topics.ClaimEvents, poisonKey, "this is not json");

        var dead = Consume(Topics.ClaimEventsDeadLetter, r => r.Message.Key == poisonKey, c => c.Count >= 1).Single();
        Assert.Equal("this is not json", dead.Message.Value);
        Assert.NotEmpty(dead.Message.Headers.GetLastBytes("error"));

        // The partition is not blocked: a normal event after the poison message is still handled.
        var claim = await InfoRequestedAsync();
        Assert.Single(await NotificationsEventuallyAsync(DemoUsers.Alice, claim.Id, 1));
    }
}
