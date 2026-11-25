# RFC-0003: Officer workload and liability exposure views

| | |
| --- | --- |
| **Status** | Implemented |
| **Author(s)** | |
| **Created** | 2026-11-25 |
| **Updated** | 2026-11-25 |
| **Related** | [RFC-0001](0001-backend-architecture.md), [RFC-0002](0002-sync-vs-async-communication.md), [ADR-0008](../adr/0008-authorisation-by-role-and-ownership-no-rebac.md), [ADR-0009](../adr/0009-authentication-deferred.md), [ADR-0011](../adr/0011-minimal-claim-data-model.md), [ADR-0012](../adr/0012-claims-handling-rules-left-out.md), design docs [1 RTM](../design/01-requirements-traceability-matrix.md) (R8) and [3 ERD](../design/03-erd.md) |

## Summary

Answer D6 and D7 with **read-only queries over the tables we already have**. No new tables, no projection and no
Kafka consumer. For the officer's own work (D6, operational), the claim list gains **time in current status** and a
sort on it, and the staff queue splits into "needs me", "waiting on claimant" and "unassigned". For the team (R8,
analytics), one workload endpoint returns open work and recent performance per officer, derived from the status
history. For managers' "outstanding liability exposure" (D7), one exposure endpoint sums **`claimed_amount` over
open claims**, broken down by status, type and age. That is the only money figure ADR-0011 keeps, and it is reported
as an upper bound, not as a reserve.

## Problem / motivation

The brief's background: staff have "no consolidated view of their workload", and managers have "no real-time picture
of outstanding claims or liability exposure". R8 asks that staff "see their team's workload and performance". The
brief then asks two design questions:

- **D6:** what does a claims officer need to see to manage their workload effectively?
- **D7:** what does the system need to know about outstanding liability exposure?

Today the API lists claims filtered by assignee, status or unassigned, ordered by `submitted_at`. The response has no
notion of how long a claim has been stuck, so nothing tells an officer what is going stale. There is no team or
exposure view. RFC-0001 step 5 (the R8 workload query) is waiting on this RFC, and RTM R8 is "Not run".

## Goals and non-goals

**Goals**
- An officer can see, at a glance, what needs their action and what is getting old.
- The team's open work and recent performance per officer (R8).
- An exposure figure that is honest about what it is built from.
- Backend first: each view is an endpoint with tests. The screens follow if time permits.

**Non-goals**
- Deadlines, SLA breaches or overdue flags. Working-day rules were left out (ADR-0012); we show age, not lateness.
- Reserves, payments, an assessed amount, or currency conversion (ADR-0011).
- A manager role. The roles stay Claimant and Officer (ADR-0008); officers see the team and exposure views.
- A reporting store, materialised views, or Kafka-fed projections.
- Charts. Tables and numbers are enough.

## Proposal

### 1. D6, operational: the officer's queue

What an officer needs to decide "what do I do next":

| Need | How |
| --- | --- |
| Work waiting on me | My claims in `UnderReview` |
| Work waiting on the claimant | My claims in `InfoRequested`. Visible, but not my action |
| New work to pick up | Unassigned claims in `Submitted` |
| What is going stale | **Time in current status** on every row, oldest first |
| How much I have | Counts per tab |

**API change** (`GET /api/claims`, additive):

- `ClaimSummaryResponse` gains `statusChangedAt`: the `changed_at` of the claim's latest status history row.
  Computed in the query (`StatusHistory.Max(h => h.ChangedAt)`). No new column, so the history stays the only source.
- New optional `sort` parameter: `submittedAt` (default, today's behaviour) or `statusChangedAt` (oldest first).
- Counts are computed client-side from the lists. They are small.

**UI change** (staff queue, `mat-tab-group` over the existing `mat-table`):

- Tabs: **Needs me** (`assigneeId = me, status = UnderReview`), **Waiting on claimant** (`InfoRequested`),
  **Unassigned** (`unassigned = true`). Each tab label shows its count.
- A "In status for" column (for example "3 days"), sorted oldest first.

### 2. D6 / R8, analytics: team workload and performance

`GET /api/workload?officerId={id}` returns the workload of the acting officer's team (`team_id` on User). The caller
must be an officer (403 otherwise), named the same way as the other endpoints until authentication exists (ADR-0009).

```jsonc
{
  "teamId": "team-a",
  "asOf": "2026-11-25T10:00:00Z",
  "periodDays": 30,
  "unassigned": { "count": 4, "oldestSubmittedAt": "..." },
  "officers": [
    {
      "officerId": "...",
      "name": "Carol Wong (officer)",
      "underReview": 3,
      "infoRequested": 1,
      "oldestOpenStatusChangedAt": "...",   // what is getting stale
      "decidedInPeriod": 7,                // performance
      "settledInPeriod": 5,
      "rejectedInPeriod": 2,
      "averageDaysToDecision": 4.2         // submitted_at → decided_at, decisions in the period
    }
  ]
}
```

- **Workload** = open claims per officer by status, plus the oldest. **Performance** = decisions in the last 30 days
  and the average time from submission to decision.
- Unassigned claims are a team-level figure; they belong to nobody yet.
- One LINQ query per figure group over `claims` and `claim_status_history`. At demo scale no caching is needed.

### 3. D7: liability exposure

What the system needs to know, and what it has:

| To know | Needed data | Have it? |
| --- | --- | --- |
| Which claims are still open | Status | Yes |
| How much each might cost | An expected amount (reserve) | **No.** Only `claimed_amount` (ADR-0011) |
| Whether the claim is covered at all | Policy and coverage | No (ADR-0011) |
| In which currency | Market and currency | No (ADR-0011). Assumed single currency |
| How old the exposure is | `submitted_at` | Yes |

So the figure this system can give is **claimed exposure**: the sum of `claimed_amount` on claims that are not yet
decided (`Submitted`, `UnderReview`, `InfoRequested`). It is an upper bound on what will be paid, because claims get
rejected or settled for less. The API and screen label it "claimed exposure", not "liability" or "reserve".

`GET /api/exposure?officerId={id}` (officers only, the whole book, not one team):

```jsonc
{
  "asOf": "...",
  "openClaims": 23,
  "claimedExposure": 184250.00,
  "byStatus": [ { "status": "Submitted", "count": 4, "claimedAmount": 31000.00 }, ... ],
  "byType":   [ { "type": "Motor", "count": 15, "claimedAmount": 96000.00 }, ... ],
  "byAge":    [ { "bucket": "0-7 days", "count": 10, "claimedAmount": 52000.00 }, ... ]   // since submitted_at
}
```

- `byStatus` shows how much is not yet picked up (`Submitted`), so nobody has looked at it.
- `byAge` uses fixed buckets (0–7, 8–30, 31+ days since submission). They are display buckets, not deadlines.
- Settled and rejected totals are left out. `claimed_amount` is not what was paid, so a "paid" figure would mislead.

### 4. Freshness

Both views are computed on each request, so they are current as of `asOf`. The SPA refreshes them with the same
polling as RFC-0002 (TanStack Query `refetchInterval`). That meets the brief's "real-time picture" at this scale
without a push channel.

### 5. Where the code goes

- `Application/Workload/WorkloadService.cs` and `Application/Exposure/ExposureService.cs`, response records next to
  them, thin `WorkloadController` and `ExposureController` (ADR-0006, ADR-0007).
- Reads use `AsNoTracking()`. No domain changes.
- Frontend: `core/api/workload.queries.ts` and `exposure.queries.ts`, a staff **Team** page with a workload
  `mat-table` and exposure figures, and the queue tabs above.

## Alternatives considered

| Option | Pros | Cons |
| --- | --- | --- |
| `status_changed_at` column on Claim | Simple sort, easy to index | Duplicates the history; one migration; the domain must keep it in step |
| Kafka-fed projection (RFC-0002 events) into workload and exposure tables | Shows events in use; scales | Second copy of the data, eventual consistency, a consumer and tables to test; no benefit at this size |
| PostgreSQL materialised views | Fast reads | Refresh scheduling; harder to test and explain than LINQ |
| Add a reserve (officer-set expected amount) | A real exposure figure | Reverses ADR-0011; a field, validation, an endpoint and UI to set it, and a rule for when it changes |
| Count claims only, no money | Nothing misleading | Does not answer D7, which is about money |
| SLA / overdue flags | What managers really want | Needs the deadline rules ADR-0012 left out |
| A Manager role for the exposure view | Matches the brief's background | A third role across auth checks, seed data and UI for a read-only page |

The proposal keeps the data model as it is (ADR-0011), so D6 and D7 cost two endpoints, one response field and one
sort option, plus screens.

## Risks, trade-offs and open questions

1. **Claimed exposure overstates liability (decided: keep claimed exposure).** It is labelled as such, and the
   reserve is the "with more time" fix. Adding a reserve now was considered and not chosen: it would reverse ADR-0011
   and cost a field, validation, an endpoint and a screen.
2. **Performance metrics (decided: as proposed).** R8 names "performance" without figures. The owner confirmed
   decisions in 30 days, settled vs rejected, and average days to decision.
3. **Who sees exposure (decided: officers, no Manager role).** Officers see the whole book's exposure and their
   team's workload. Without a manager role, that is the nearest reading of the brief.
4. Single currency is assumed. Six markets means this is wrong in reality; recorded, not solved (ADR-0011).
5. Age is calendar days, not working days (ADR-0012).
6. The subquery for `statusChangedAt` runs per row. Fine for a demo; an index on `claim_status_history (claim_id,
   changed_at)` is cheap if needed.

## Rollout / implementation plan

1. `statusChangedAt` and the `sort` parameter on `GET /api/claims`, with integration tests (R5/R8).
2. `GET /api/workload` with integration tests over seeded claims in several states (R8).
3. `GET /api/exposure` with integration tests, including that decided claims are excluded (D7).
4. Regenerate the OpenAPI contract and frontend types.
5. Frontend: queue tabs and the "In status for" column; the Team page with workload and exposure; unit and e2e tests.
6. Update the RTM (R8 implementation and tests), the use cases (UC-08 built), the scope doc (D6, D7) and RFC-0001
   step 5.

Steps 1–3 can be built before RFC-0002. Polling (section 4) arrives with RFC-0002's frontend step.

## Outcome

Implemented as proposed. [ADR-0014](../adr/0014-workload-views-computed-on-read-from-status-history.md) (workload
views computed on read from the status history) and [ADR-0015](../adr/0015-exposure-is-claimed-amount-on-open-claims.md)
(exposure is the claimed amount on open claims) are Accepted. Built: `statusChangedAt` and `sort` on
`GET /api/claims`, `GET /api/workload`, `GET /api/exposure` (one `TeamController`), the three-tab queue with an
"In status for" column, and the staff Team page (`/work/team`). The database filters and the figures are grouped in
memory, which is fine at demo scale. Exposure tests use a `D7_` prefix (ADR-0020 amendment).
