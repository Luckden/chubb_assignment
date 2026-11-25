# ADR-0018: Claimant notifications are built by a Kafka consumer

- **Status:** Proposed
- **Date:** 2026-11-25
- **Deciders:** Project owner
- **Related:** [RFC-0002](../rfcs/0002-sync-vs-async-communication.md), [ADR-0010](0010-information-requests-as-structured-questions-not-chat.md), [ADR-0016](0016-rest-for-commands-kafka-for-lifecycle-events-via-outbox.md)

## Context

The events need a reader that a user can see, and R3 and R4 had no "you were told" step: the claimant only found out
by opening the claim. The brief does not name notifications.

## Decision

- An in-process consumer (`KafkaConsumerService`) dispatches events to `IEventHandler`s. `ClaimantNotificationHandler`
  turns `InformationRequested`, `ClaimSettled` and `ClaimRejected` into a row in `notifications`.
- The unique `event_id` on that table makes redelivery harmless.
- `GET /api/notifications?userId=` and `POST /api/notifications/{id}/read` (body names the user, as in ADR-0009).
  Only the recipient can mark a notification read.
- The SPA shows a bell with an unread count for claimants, refreshed by polling (ADR-0017).
- No email, SMS or push (ADR-0010).

## Consequences

- R3 and R4 have a visible notification, and the Kafka split has a real reader.
- The consumer shares the API process and database, so it cannot be scaled or deployed apart. Fine for this scope.
- An audit-log consumer was the smaller option and was not chosen: it gives the user nothing to see.

## Alternatives considered

- **No consumer:** least work, but the REST/Kafka split is hard to justify with no reader.
- **A workload/statistics projection (R8) as the consumer:** a SQL query over the status history is simpler (ADR-0014).
