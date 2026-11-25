# RFCs (Requests for Comments)

Proposals that are discussed before they are built. See the [docs README](../README.md) for how RFCs lead to ADRs.

## Process

1. Copy [0000-template.md](0000-template.md) to `NNNN-short-title.md` (next free number, 4 digits, kebab-case).
2. Fill it in with status **Draft** and add it to the index below.
3. Discuss (PR review, comments). Revise.
4. Set the final status: **Accepted**, **Rejected** or **Withdrawn**. Keep rejected RFCs; they record what was considered.
5. For an Accepted RFC, write the resulting ADR(s) and link them in the RFC's *Outcome* section.

Statuses: `Draft` → `In Review` → `Accepted` | `Rejected` | `Withdrawn`, then `Implemented` once built

## Index

| # | Title | Status | Resulting ADRs |
| --- | --- | --- | --- |
| [0001](0001-backend-architecture.md) | Backend architecture (.NET Web API and EF Core) | Accepted | [0006](../adr/0006-backend-layered-structure.md), [0007](../adr/0007-application-services-and-ef-core-conventions.md) |
| [0002](0002-sync-vs-async-communication.md) | Synchronous vs asynchronous communication (REST and Kafka) | Implemented | [0016](../adr/0016-rest-for-commands-kafka-for-lifecycle-events-via-outbox.md), [0017](../adr/0017-frontend-learns-of-changes-by-polling.md), [0018](../adr/0018-claimant-notifications-built-by-a-kafka-consumer.md), [0019](../adr/0019-snake-case-database-names.md), [0020](../adr/0020-constraint-traced-tests-use-c-prefix.md) |
| [0003](0003-officer-workload-and-liability-exposure.md) | Officer workload and liability exposure views | Implemented | [0014](../adr/0014-workload-views-computed-on-read-from-status-history.md), [0015](../adr/0015-exposure-is-claimed-amount-on-open-claims.md) |
