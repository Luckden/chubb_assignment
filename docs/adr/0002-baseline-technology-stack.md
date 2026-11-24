# ADR-0002: Baseline technology stack

- **Status:** Proposed <!-- review and accept, or amend, before building on it -->
- **Date:** 2026-11-24
- **Deciders:** Project owner
- **Related:** —

## Context

The brief prefers Angular for the frontend, C#/.NET or Java/Spring Boot for the backend, and REST/HTTP plus Kafka
for communication. Everything else is open. The app must start locally and must not be publicly hosted.

## Decision

| Concern | Choice | Notes |
| --- | --- | --- |
| Backend | .NET 10 (LTS), ASP.NET Core controllers | `backend/`; SDK pinned in `global.json` |
| Persistence | PostgreSQL 18 via EF Core 10 + Npgsql | EF Core 11 / Npgsql 11 target .NET 11 (STS), so they are not used |
| Messaging | Apache Kafka 4.1 (single node, KRaft) via Confluent.Kafka | Kafka UI on :8081 for inspection |
| Frontend | Angular 21 (current LTS line), standalone, zoneless, signals, Vitest | Angular 22 is newer but in active, not LTS, support |
| End-to-end tests | Playwright (Chromium) in `frontend/e2e` | API mocked by default; `@stack` specs run against Docker Compose |
| Runtime for frontend tooling | Node.js 24 LTS | `.nvmrc` |
| Local orchestration | Docker Compose | `docker-compose.yml` at the repo root |
| API description | Built-in ASP.NET Core OpenAPI (`/openapi/v1.json`) | |

Service boundaries, the REST/Kafka split and the data model are **not** decided here; they are for later RFCs.

## Consequences

- One `docker compose up` brings up the whole stack; infrastructure can also be started alone while running the
  API and SPA from the IDE.
- LTS versions everywhere reduce churn during the assessment window.

## Alternatives considered

- **Java/Spring Boot**: equally allowed; .NET chosen for familiarity (owner to confirm / amend).
- **ZooKeeper-based Kafka**: removed in Kafka 4; KRaft is the only mode.
- **SQL Server**: heavier container; PostgreSQL is lighter and has first-class EF Core support.
