# RFC-0001: Domain model and service boundaries

| | |
| --- | --- |
| **Status** | Draft |
| **Author(s)** | |
| **Created** | 2026-11-25 |
| **Updated** | 2026-11-25 |
| **Related** | [RFC-0002](0002-sync-vs-async-communication.md) |

## Summary

_TODO: one paragraph once the proposal is written._

## Problem / motivation

Claims arrive by phone and email. Staff work from shared inboxes and spreadsheets, and managers can't see what is
outstanding or how much liability is exposed. We need a domain model and service decomposition that supports:

- **Claimants:** report an incident, track the claim, provide more information when asked, receive decisions.
- **Claims staff:** pick up incoming claims, review and assess them, settle or reject, see team workload and performance.
- **Managers:** real-time view of outstanding claims and liability exposure.
- Motor and property claims across six markets.

## Questions this RFC must answer (from the brief)

- [ ] What are the core entities, and how do they relate?
- [ ] What is the claim lifecycle (states, transitions, who can trigger each)?
- [ ] Is the backend one service or several? What drives that decision?
- [ ] What does the system need to know about outstanding liability exposure (reserves, currency, per market)?
- [ ] What does a claims officer need to see to manage their workload?

## Goals and non-goals

**Goals**
-

**Non-goals**
-

## Proposal

_TODO._ Candidate sections: entity diagram, lifecycle state machine, aggregate boundaries, service map.

## Alternatives considered

| Option | Pros | Cons |
| --- | --- | --- |
| Modular monolith (one deployable, clear modules) | | |
| Separate services (e.g. intake, claims handling, reporting) | | |

## Risks, trade-offs and open questions

-

## Rollout / implementation plan

-

## Outcome

_Filled in when the RFC is closed._
