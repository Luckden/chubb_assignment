using Claims.Api.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Claims.Api.Infrastructure.Messaging;

/// <summary>
/// An event waiting to be published to Kafka. Written in the same transaction as the state change, so an event is
/// never lost and never published for a change that rolled back (RFC-0002 section 4).
/// </summary>
public sealed class OutboxMessage
{
    private OutboxMessage()
    {
    }

    /// <summary>Equals the event id in the envelope.</summary>
    public Guid Id { get; private set; }

    public string Topic { get; private set; } = string.Empty;

    /// <summary>The Kafka message key: the claim id.</summary>
    public string Key { get; private set; } = string.Empty;

    public string EventType { get; private set; } = string.Empty;

    /// <summary>The serialised <see cref="EventEnvelope"/>.</summary>
    public string Payload { get; private set; } = string.Empty;

    public DateTimeOffset OccurredAt { get; private set; }

    /// <summary>Null until Kafka acknowledged the message.</summary>
    public DateTimeOffset? PublishedAt { get; private set; }

    public static OutboxMessage For(ClaimDomainEvent @event)
    {
        var id = Guid.CreateVersion7();
        return new OutboxMessage
        {
            Id = id,
            Topic = Topics.ClaimEvents,
            Key = @event.ClaimId.ToString(),
            EventType = @event.EventType,
            Payload = EventEnvelope.From(id, @event).ToJson(),
            OccurredAt = @event.OccurredAt,
        };
    }
}

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedNever();
        builder.Property(m => m.Topic).HasMaxLength(200).IsRequired();
        builder.Property(m => m.Key).HasMaxLength(100).IsRequired();
        builder.Property(m => m.EventType).HasMaxLength(100).IsRequired();
        builder.Property(m => m.Payload).IsRequired();

        // The relay only reads unpublished rows, oldest first.
        builder.HasIndex(m => new { m.OccurredAt, m.Id }).HasFilter("published_at IS NULL");
    }
}
