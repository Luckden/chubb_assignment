# ADR-0014: Workload views are computed on read from the status history

- **Status:** Proposed
- **Date:** 2026-11-25
- **Deciders:** Project owner
- **Related:** [RFC-0003](../rfcs/0003-officer-workload-and-liability-exposure.md), [ADR-0011](0011-minimal-claim-data-model.md), [ADR-0012](0012-claims-handling-rules-left-out.md), [Design doc 3](../design/03-erd.md)

## Context

D6 asks what an officer needs to see to manage their workload, and R8 asks that staff see their team's workload and
performance. The claim list today has no measure of how long a claim has been waiting, so an officer cannot tell what
is going stale. The status history table was kept (ERD decision 2) so that figures like these can be derived.

## Decision

- An officer's queue shows **time in current status**. `ClaimSummaryResponse` gains `statusChangedAt`, the time of the
  claim's latest status history row, and `GET /api/claims` gains a `sort=statusChangedAt` option. No new column.
- The staff queue splits into "Needs me" (`UnderReview`), "Waiting on claimant" (`InfoRequested`) and "Unassigned".
- `GET /api/workload` returns the acting officer's team: open claims per officer by status and the oldest of them,
  unassigned claims, and performance over the last 30 days (decisions, settled vs rejected, average days from
  submission to decision). Officers only.
- All of it is computed on each request with LINQ over `claims` and `claim_status_history`. No projection tables,
  no materialised views and no Kafka consumer.
- Age is shown in calendar days, with no deadline or overdue flag.

## Consequences

- No schema change and no second copy of the data, so the figures cannot drift from the claims.
- Answers R8 and unblocks RFC-0001 step 5.
- Each request runs aggregate queries. Fine at demo scale; an index on `claim_status_history (claim_id, changed_at)`
  or a projection would be the next step at real volume.
- The performance figures are a choice; R8 does not name them.
- **With more time:** due dates and an overdue flag once deadline rules exist (ADR-0012), and a projection fed by the
  RFC-0002 events if the queries get slow.

## Alternatives considered

- **`status_changed_at` column on Claim:** easier to sort and index, but duplicates the history and needs a migration.
- **Kafka-fed projection tables:** shows events in use, but adds eventual consistency and a consumer for no gain here.
- **Materialised views:** fast, but need refresh scheduling and are harder to test than LINQ.
- **SLA and overdue flags:** what a manager wants, but depend on the deadline rules left out in ADR-0012.
