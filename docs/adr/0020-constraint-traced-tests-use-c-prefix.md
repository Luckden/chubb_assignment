# ADR-0020: Tests for infrastructure constraints use a `C{n}_` prefix

- **Status:** Accepted
- **Date:** 2026-11-25
- **Deciders:** Project owner
- **Related:** [ADR-0013](0013-test-conventions-and-single-test-project.md), [RFC-0002](../rfcs/0002-sync-vs-async-communication.md), [scope and prioritisation](../scope-and-prioritisation.md)

## Context

ADR-0013 names every test `R{requirement}_...` so it traces back to the RTM. Some behaviour has no requirement of its
own: it exists because the brief constrains the solution (constraint C3, "REST/HTTP and Kafka"). Examples are the outbox
publishing events in order, the consumer's duplicate handling and the dead-letter topic. Forcing an `R` id on them
would point at the wrong requirement.

## Decision

- A test for behaviour that exists only because of a constraint in `scope-and-prioritisation.md` is named
  `C{constraint}_{Method}_{Scenario}_{ExpectedBehavior}`, for example `C3_Outbox_ClaimLifecycle_PublishesEventsInOrderKeyedByClaim`.
- Everything else in ADR-0013 applies unchanged: one act per test, named constants, no logic.
- A test that serves a requirement stays `R{n}_`. Event tests that exist to deliver a requirement (for example the
  notification for R3) use `R{n}_`.
- The RTM lists `C{n}_*` tests in the Notes column of the requirement they support, if any.

## Consequences

- Infrastructure tests trace to the brief instead of to an unrelated requirement.
- Two prefixes to remember; the existing Kafka round-trip test is renamed to follow it.

## Alternatives considered

- **Tag with an `R` id anyway:** keeps one rule, but traces to a requirement the test does not cover.
- **No prefix for infrastructure tests:** loses traceability.
