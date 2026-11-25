# ADR-0013: Test conventions and a single test project

- **Status:** Accepted
- **Date:** 2026-11-25
- **Deciders:** Project owner
- **Related:** [ADR-0003](0003-engineering-guardrails-for-ai-assisted-development.md), [Design doc 1](../design/01-requirements-traceability-matrix.md)

## Context

Tests need to trace back to requirements and follow Microsoft's unit testing guidance
(`MethodName_Scenario_ExpectedBehavior`, Arrange-Act-Assert, one act, no logic, no magic strings). ADR-0003 already
requires real infrastructure in tests, not mocks.

## Decision

- Name tests `R{requirement}_{Method}_{Scenario}_{ExpectedBehavior}`. The requirement id gives backward traceability
  from the RTM.
- One act per test. Arrange through helpers, use named constants and the simplest inputs, and use `[Theory]` for
  similar cases.
- Unit tests (`Domain/`, `Application/`) use no infrastructure. Integration tests use real PostgreSQL and Kafka through
  Testcontainers.
- **One test project** (`Claims.Api.Tests`) for both kinds.
- Where the host blocks test DLLs, run `scripts/test-backend-in-docker.sh`.

## Consequences

- Every test links to a requirement, and the RTM names them by pattern.
- Unit tests share a project with Testcontainers packages, which the Microsoft guidance advises against.
- **With more time:** split into unit and integration projects (and CI jobs), and measure coverage (coverlet is already
  referenced).

## Alternatives considered

- **Two projects now:** matches the guidance, but changes CI, the Docker script and AGENTS.md for little gain.
- **Mocks or in-memory EF:** faster, but hide real behaviour. ADR-0003 rejects them.
