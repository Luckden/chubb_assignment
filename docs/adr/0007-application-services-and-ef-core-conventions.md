# ADR-0007: Application services, no repositories, rich domain and EF Core conventions

- **Status:** Accepted
- **Date:** 2026-11-25
- **Deciders:** Project owner
- **Related:** [RFC-0001](../rfcs/0001-backend-architecture.md), [ADR-0006](0006-backend-layered-structure.md)

## Context

[ADR-0006](0006-backend-layered-structure.md) fixed the folders and the dependency direction. The remaining RFC-0001
choices decide how the code inside them is written: where the lifecycle rules live, how persistence is reached, and how
EF Core is configured.

## Decision

- **Application services.** One class per feature (`ClaimService`) with one method per use case. No mediator library.
- **No repositories.** Application services use `ClaimsDbContext` directly. `Domain` stays free of EF types.
- **Rich domain entities.** `Claim` owns its lifecycle: `Submit`, `PickUp`, `RequestInformation`, `ProvideInformation`,
  `Settle`, `Reject`. An illegal move throws `InvalidTransitionException` (409). A wrong user throws
  `NotPermittedException` (403). Each transition adds a `ClaimStatusChange`.
- **Errors as ProblemDetails.** One `IExceptionHandler` maps validation, not-found, domain and concurrency exceptions.
  Validation uses one FluentValidation validator per request DTO, called from the service.
- **EF Core conventions.**
  - One `IEntityTypeConfiguration<T>` per entity.
  - Enums stored as strings.
  - Keys are Guid v7 created in the domain, with `ValueGeneratedNever()` so EF does not mistake a new child for an
    existing one.
  - `Claim.Version` is a row version mapped to PostgreSQL `xmin`, so two officers cannot both pick up a claim.
  - Migrations and the idempotent seeder run at startup in Development and tests.
- **No authentication yet.** Requests name the acting user and the service checks the user exists and has the role.
  This is a placeholder, not a security control, and is replaced when an auth approach is decided.

## Consequences

- Rules live in one place and are unit-testable without a database (15 domain tests).
- `Application` depends on EF Core. That is accepted; only `Domain` is kept free of it.
- A row version sits on a domain entity, a small persistence concern in `Domain`.
- Tests that need the database run against real PostgreSQL (Testcontainers), including a concurrent pick-up test.
- Anyone can act as any user until auth exists, so the API is not safe to expose.

## Alternatives considered

- **Mediator and command/handler classes:** uniform shape, but extra dependency and indirection for about a dozen use cases.
- **Repository interfaces:** `DbContext` already is a unit of work, and tests use real Postgres anyway.
- **Anemic entities with rules in services:** rules scatter and illegal transitions are easier to introduce.
- **Minimal APIs:** less boilerplate, but the scaffold, OpenAPI conventions and AGENTS.md assume controllers.
