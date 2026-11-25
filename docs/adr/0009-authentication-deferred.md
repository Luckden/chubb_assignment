# ADR-0009: Authentication deferred; requests name the acting user

- **Status:** Accepted
- **Date:** 2026-11-25
- **Deciders:** Project owner
- **Related:** [RFC-0001](../rfcs/0001-backend-architecture.md), [ADR-0007](0007-application-services-and-ef-core-conventions.md), [ADR-0008](0008-authorisation-by-role-and-ownership-no-rebac.md)

## Context

The brief does not mention authentication. Real login (identity provider, tokens, sign-in screens) is a large piece
of work that does not show the claims domain, REST/Kafka split or data model, which are what is assessed.

## Decision

- No authentication. Each request names the acting user (`claimantId` or `officerId`).
- The service checks that the user exists and has the right role, then the domain checks ownership (ADR-0008).
- Four demo users are seeded with fixed ids (two claimants, two officers) in Development and tests.

## Consequences

- The API can be exercised and tested end to end with no setup.
- **Anyone can act as anyone, so the API is not safe to expose.** It is a placeholder, not a security control.
- Reads are unrestricted, because there is no caller identity to filter by.
- **With more time:** JWT or OIDC sign-in, take the acting user from the token (an `ICurrentUser`), remove the user ids
  from request bodies, and filter reads by the caller.

## Alternatives considered

- **ASP.NET Identity or JWT with seeded users:** about half an hour for the API, plus sign-in screens in the SPA.
- **A demo header such as `X-Demo-User`:** keeps ids out of bodies, but is the same placeholder with more plumbing.
- **A hosted identity provider:** realistic, but far beyond the sprint.
