# ADR-0008: Authorisation by role and ownership checks, no ReBAC

- **Status:** Accepted
- **Date:** 2026-11-25
- **Deciders:** Project owner
- **Related:** [RFC-0001](../rfcs/0001-backend-architecture.md), [ADR-0007](0007-application-services-and-ef-core-conventions.md), [ADR-0009](0009-authentication-deferred.md)

## Context

The brief says nothing about access control, but the domain has three relationships: a claimant owns claims, an officer
is assigned to claims, and officers belong to a team. A relationship-based access control (ReBAC) engine such as
OpenFGA or SpiceDB, or a set of ASP.NET `IAuthorizationHandler` policies, could model these. The time budget is 2 to 3
hours and the backend is what is assessed.

## Decision

- Two roles on `User`: Claimant and Officer. Managers are not a separate role; team workload sits under claims staff.
- The rules are plain checks in the domain and the service:
  - only a claimant can report a claim, and only the owning claimant can answer an information request;
  - any officer can pick up an unassigned claim;
  - only the assigned officer can ask for information, settle or reject.
- A broken rule returns 403 (`NotPermittedException`). No ReBAC engine and no authorisation policy classes.

## Consequences

- Small, tested (403 cases in the domain and API tests) and easy to explain.
- Reading is not restricted: any caller can fetch any claim or list. That needs an authenticated identity first
  (ADR-0009).
- **With more time:** policy handlers for read access, then ReBAC if the rules grow into sharing, delegation or
  market-scoped team hierarchies.

## Alternatives considered

- **OpenFGA or SpiceDB (ReBAC):** powerful, but another service to run and explain for three relationships.
- **`IAuthorizationHandler` policies:** idiomatic, but more ceremony than the checks above and useful only once
  there is an authenticated user.
