# RFC-0002: Synchronous vs asynchronous communication (REST and Kafka)

| | |
| --- | --- |
| **Status** | Draft |
| **Author(s)** | |
| **Created** | 2026-11-25 |
| **Updated** | 2026-11-25 |
| **Related** | [RFC-0001](0001-domain-model-and-service-boundaries.md) |

## Summary

_TODO: one paragraph once the proposal is written._

## Problem / motivation

The brief requires REST/HTTP and Kafka, and asks us to choose and justify what uses which. Claims processing is
asynchronous by nature (assessment, requests for information, decisions), and both the backend and the Angular
frontend have to handle that.

## Questions this RFC must answer (from the brief)

- [ ] Which operations must be synchronous (the caller needs an answer now)?
- [ ] Which can be event-driven, and who consumes the events?
- [ ] What is the contract between frontend and backend, and who defines it?
- [ ] How does the frontend learn that something changed asynchronously (polling, SSE, SignalR/WebSockets)?
- [ ] How do the database write and the Kafka publish stay consistent (outbox? dual write?)
- [ ] Delivery semantics: idempotent consumers, ordering (message key), retries, dead-letter topics.

## Current baseline (already in the scaffold)

- REST: ASP.NET Core controllers; the OpenAPI document is exported on build to `backend/openapi/claims-api.json`,
  and the frontend types are generated from it (`npm run api:generate`). **The backend owns the contract.**
- Kafka: `IEventPublisher` (idempotent producer, `acks=all`, JSON payloads keyed by aggregate id) and a consumer
  background service skeleton.

## Proposal

_TODO._ Candidate sections: interaction table (operation → REST or event), topic and event catalogue, consistency
approach, frontend update mechanism.

| Interaction | Style | Why |
| --- | --- | --- |
| | | |

## Alternatives considered

| Option | Pros | Cons |
| --- | --- | --- |
| | | |

## Risks, trade-offs and open questions

-

## Outcome

_Filled in when the RFC is closed._
