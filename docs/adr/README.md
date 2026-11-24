# Architecture Decision Records

Short records of significant decisions, in the style of Michael Nygard's ADRs.
See the [docs README](../README.md) for how ADRs relate to RFCs.

## Process

1. Copy [0000-template.md](0000-template.md) to `NNNN-short-title.md` (next free number, 4 digits, kebab-case).
2. Write it with status **Proposed**, link the RFC it came from (if any), and add it to the index below.
3. Once agreed, set it to **Accepted**. From then on the content does not change.
4. To reverse or change a decision, write a new ADR and mark the old one `Superseded by ADR-NNNN`.

Statuses: `Proposed` → `Accepted` → (`Deprecated` | `Superseded by ADR-NNNN`), or `Rejected`.

## Index

| # | Title | Status |
| --- | --- | --- |
| [0001](0001-record-architecture-decisions.md) | Record architecture decisions | Accepted |
| [0002](0002-baseline-technology-stack.md) | Baseline technology stack | Proposed |
| [0003](0003-engineering-guardrails-for-ai-assisted-development.md) | Engineering guardrails for AI-assisted development | Proposed |
