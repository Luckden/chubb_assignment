# ADR-0017: The frontend learns of changes by polling

- **Status:** Proposed
- **Date:** 2026-11-25
- **Deciders:** Project owner
- **Related:** [RFC-0002](../rfcs/0002-sync-vs-async-communication.md), [ADR-0004](0004-frontend-ui-and-server-state-libraries.md), [ADR-0016](0016-rest-for-commands-kafka-for-lifecycle-events-via-outbox.md)

## Context

Claims change asynchronously: an officer asks a question while the claimant has the claim open. The SPA needs to see
that without a reload.

## Decision

TanStack Query polls with `refetchInterval` of 10 s (`core/api/polling.ts`) on the claim list, claim detail and
notification queries. `refetchIntervalInBackground` is off, so hidden tabs stop polling. Mutations still invalidate
their keys.

## Consequences

- One option per query, easy to test, no second channel.
- Up to 10 s delay. A poll per open tab is fine for a demo, not at production scale.
- **With more time:** SSE or SignalR fed by the notifications consumer.

## Alternatives considered

- **SSE or SignalR:** instant, but adds reconnection handling and a fan-out consumer.
- **Refetch on focus only (the previous behaviour):** misses changes while the user is looking at the screen.
