# Project scope statement: scope and prioritisation

This page records what the brief asks for: the objectives, deliverables and constraints, and the design questions it
poses. The requirements themselves are traced in a design document.

> Status: all seven design questions are answered and all eight requirements are built. Every requirement is Must in
> the RTM: each one is required by the assignment.

## Objectives

Drafted from the brief; edit freely.

- Build a claims platform where claimants report and follow a claim, and claims staff process claims through to a decision.
- Spend most of the effort on a well-built backend; extend to the frontend if time permits.
- Make the design decisions (service boundaries, REST vs Kafka, data model, priorities) explainable.

## Deliverables

From the brief's Deliverables section.

- Git repository with meaningful commit history.
- Working application that can be started locally.
- AI working journal (`docs/ai-journal.md`).
- Supporting documentation.

## Constraints

From the brief (Technology, Approach, Time guidance, Disclaimer).

| ID | Constraint | Source (brief) |
| --- | --- | --- |
| C1 | Angular frontend; one app for both user types | Technology; "same Angular application" |
| C2 | C#/.NET or Java/Spring Boot backend | Technology |
| C3 | REST/HTTP and Kafka, with the split explained | Technology |
| C4 | Backend first; frontend if time permits | Approach |
| C5 | No public hosting; no Chubb branding or proprietary material | Disclaimer |
| C6 | 2–3 hour target, 5 hour hard cap | Time guidance |

## Requirements traceability matrix

See [design/01-requirements-traceability-matrix.md](design/01-requirements-traceability-matrix.md).

## Design questions the brief asks me to answer

These come from the brief's "questions to consider". They are not requirements, so they are not in the RTM.

| ID | Question | Answered in | Status |
| --- | --- | --- | --- |
| D1 | Core entities and how they relate | [Design doc 3](design/03-erd.md) | Done |
| D2 | One service or more, and what drives that | [RFC-0001](rfcs/0001-backend-architecture.md) | Done |
| D3 | What is synchronous and what is event-driven | [RFC-0002](rfcs/0002-sync-vs-async-communication.md), ADR-0016 | Built |
| D4 | Frontend–backend contract and who defines it | OpenAPI exported on build, backend-owned (AGENTS.md) | Done |
| D5 | How the async nature of claims processing is handled across both tiers | [RFC-0002](rfcs/0002-sync-vs-async-communication.md), ADR-0017, ADR-0018 | Built |
| D6 | What an officer needs to see to manage workload | [RFC-0003](rfcs/0003-officer-workload-and-liability-exposure.md), ADR-0014 | Done |
| D7 | What the system needs to know about liability exposure | [RFC-0003](rfcs/0003-officer-workload-and-liability-exposure.md), ADR-0015 | Done |
