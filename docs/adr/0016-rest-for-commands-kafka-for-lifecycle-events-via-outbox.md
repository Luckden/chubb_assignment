# ADR-0016: REST for commands and queries, Kafka for lifecycle events through a transactional outbox

- **Status:** Accepted
- **Date:** 2026-11-25
- **Deciders:** Project owner
- **Related:** [RFC-0002](../rfcs/0002-sync-vs-async-communication.md), [ADR-0002](0002-baseline-technology-stack.md), [ADR-0007](0007-application-services-and-ef-core-conventions.md), [ADR-0018](0018-claimant-notifications-built-by-a-kafka-consumer.md)

## Context

The brief requires REST and Kafka and asks what uses which. A claim action needs an answer now (400, 403, the
pick-up race 409). Publishing to Kafka after `SaveChanges` can lose the event or fail the REST call when the broker is
down.

## Decision

- Every user action stays a synchronous REST command or query.
- Every lifecycle transition also raises a past-tense event (`ClaimSubmitted`, `ClaimPickedUp`,
  `InformationRequested`, `InformationProvided`, `ClaimSettled`, `ClaimRejected`) on one topic,
  `claims.claim-events`, keyed by claim id so a claim's events stay in order.
- Events go to an `outbox_messages` table in the same `SaveChanges` as the state change. `OutboxRelay` publishes them
  in order and marks them published after Kafka acknowledges.
- Delivery is at least once. Consumers dedupe on the event id, commit the offset after handling, retry 3 times and
  then send the message to `claims.claim-events.dlt`.
- The API creates both topics itself (Compose auto-create is off). The producer is created on first use, so the API
  and the OpenAPI export start without a broker.

## Consequences

- No event is lost and no REST call fails because Kafka is slow or down. Published rows are kept as an event log.
- Costs one table, one migration and one hosted service.
- One API instance is assumed. With several, the relay needs `FOR UPDATE SKIP LOCKED` or a leader.
- Not built: outbox clean-up, a schema registry, outbox lag metrics (only logged).

## Alternatives considered

- **Publish after `SaveChanges`:** simplest, but loses events and couples REST to the broker.
- **Debezium CDC on the outbox:** no relay code, but another container and connector for a small build.
- **Commands over Kafka:** the caller loses the 400/403/409 answer.
- **One topic per event type:** loses ordering across a claim's events.
