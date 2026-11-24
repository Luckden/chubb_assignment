# RFCs (Requests for Comments)

Proposals that are discussed before they are built. See the [docs README](../README.md) for how RFCs lead to ADRs.

## Process

1. Copy [0000-template.md](0000-template.md) to `NNNN-short-title.md` (next free number, 4 digits, kebab-case).
2. Fill it in with status **Draft** and add it to the index below.
3. Discuss (PR review, comments). Revise.
4. Set the final status: **Accepted**, **Rejected** or **Withdrawn**. Keep rejected RFCs; they record what was considered.
5. For an Accepted RFC, write the resulting ADR(s) and link them in the RFC's *Outcome* section.

Statuses: `Draft` → `In Review` → `Accepted` | `Rejected` | `Withdrawn`

## Index

| # | Title | Status | Resulting ADRs |
| --- | --- | --- | --- |
| [0001](0001-domain-model-and-service-boundaries.md) | Domain model and service boundaries | Draft | |
| [0002](0002-sync-vs-async-communication.md) | Synchronous vs asynchronous communication (REST and Kafka) | Draft | |
