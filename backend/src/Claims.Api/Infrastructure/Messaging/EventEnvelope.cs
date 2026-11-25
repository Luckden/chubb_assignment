using System.Text.Json;
using System.Text.Json.Serialization;
using Claims.Api.Domain.Events;

namespace Claims.Api.Infrastructure.Messaging;

/// <summary>The JSON wrapper around every event on <see cref="Topics.ClaimEvents"/> (RFC-0002 section 2).</summary>
public sealed record EventEnvelope(
    Guid EventId,
    string EventType,
    int SchemaVersion,
    DateTimeOffset OccurredAt,
    Guid ClaimId,
    JsonElement Data)
{
    public const int CurrentSchemaVersion = 1;

    public static readonly JsonSerializerOptions JsonOptions = CreateOptions();

    /// <summary>Wraps a domain event. <paramref name="eventId"/> is the idempotency key consumers dedupe on.</summary>
    public static EventEnvelope From(Guid eventId, ClaimDomainEvent @event) =>
        new(
            eventId,
            @event.EventType,
            CurrentSchemaVersion,
            @event.OccurredAt,
            @event.ClaimId,
            JsonSerializer.SerializeToElement(@event, @event.GetType(), JsonOptions));

    public string ToJson() => JsonSerializer.Serialize(this, JsonOptions);

    public static EventEnvelope Parse(string json) =>
        JsonSerializer.Deserialize<EventEnvelope>(json, JsonOptions)
        ?? throw new JsonException("The message is not an event envelope.");

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerOptions.Web);
        options.Converters.Add(new JsonStringEnumConverter());
        options.MakeReadOnly(populateMissingResolver: true);
        return options;
    }
}
