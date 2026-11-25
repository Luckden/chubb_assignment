# ADR-0006: Backend as one layered project with feature folders

- **Status:** Accepted
- **Date:** 2026-11-25
- **Deciders:** Project owner
- **Related:** [RFC-0001](../rfcs/0001-backend-architecture.md), [ADR-0002](0002-baseline-technology-stack.md)

## Context

The backend needs a structure before any domain code is written. The time budget is small, the domain is four tables
and five statuses, and AGENTS.md already requires thin controllers, logic in `Application/` and a `Domain/` with no
framework dependencies. RFC-0001 compared structures and agreed this one.

## Decision

- **One deployable service** (modular monolith): `backend/src/Claims.Api`.
- **Layer-first folders** inside the single project, with feature subfolders inside `Application/`:
  `Controllers/`, `Application/<Feature>/`, `Domain/`, `Infrastructure/{Persistence,Messaging}`.
- **Dependency direction:** `Controllers` → `Application` → `Domain`. `Infrastructure` supplies persistence and
  messaging. `Domain` depends on nothing outside itself.

## Consequences

- Simple to navigate and explain, with no project-reference plumbing.
- The dependency rule is enforced by convention and review, not by the compiler. An architecture test could be added
  later if it drifts.
- Splitting into services later means extracting a feature folder and its tables. It is not free, but it is not
  blocked.
- The remaining RFC-0001 choices (application services, no repositories, rich entities, EF conventions) are
  recorded in [ADR-0007](0007-application-services-and-ef-core-conventions.md).

## Alternatives considered

- **Feature-first slices:** mixes domain and framework code, against the AGENTS.md rule.
- **Multi-project solution:** the compiler would enforce the layers, but it adds more ceremony than a sprint needs.
- **Microservices:** distributed consistency and operational cost with no benefit at this size.
