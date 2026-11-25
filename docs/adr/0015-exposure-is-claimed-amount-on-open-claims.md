# ADR-0015: Liability exposure is the claimed amount on open claims

- **Status:** Accepted
- **Date:** 2026-11-25
- **Deciders:** Project owner
- **Related:** [RFC-0003](../rfcs/0003-officer-workload-and-liability-exposure.md), [ADR-0008](0008-authorisation-by-role-and-ownership-no-rebac.md), [ADR-0011](0011-minimal-claim-data-model.md), [ADR-0014](0014-workload-views-computed-on-read-from-status-history.md)

## Context

D7 asks what the system needs to know about outstanding liability exposure. The brief's background says managers
have no real-time picture of it. A real figure needs an expected cost per claim (a reserve), coverage and currency.
ADR-0011 left all three out, so the only money figure is `claimed_amount`, and ADR-0011 already noted that exposure
could only be a sum of it.

## Decision

- **Claimed exposure** is the sum of `claimed_amount` over claims not yet decided (`Submitted`, `UnderReview`,
  `InfoRequested`). It is labelled "claimed exposure" in the API and UI, never "liability" or "reserve".
- `GET /api/exposure` returns the total and the open-claim count, broken down by status, by claim type and by age
  since submission (0–7, 8–30, 31+ days). Computed on read, like ADR-0014.
- No totals for settled or rejected claims, because `claimed_amount` is not what was paid.
- A single currency is assumed.
- Officers see it for the whole book. No Manager role is added (ADR-0008).

## Consequences

- D7 is answered without changing the data model, and the answer is honest about its limits.
- The figure is an upper bound: rejected claims and partial settlements make the real liability lower.
- Amounts from different markets would be added together as if one currency.
- **With more time:** a reserve set by the officer while assessing (superseding ADR-0011's "no assessed amount"),
  a paid amount on settlement, currency per claim, and a Manager role for this view.

## Alternatives considered

- **Add a reserve now:** a real exposure figure, but reverses ADR-0011 and needs a field, validation, an endpoint
  and UI to set it.
- **Claim counts only:** nothing misleading, but D7 is about money.
- **A Manager role:** matches the brief's background, but adds a third role to auth checks, seed data and UI for one
  read-only page.
