# ADR-0012: Real-world claims handling rules are left out

- **Status:** Accepted
- **Date:** 2026-11-25
- **Deciders:** Project owner
- **Related:** [Design doc 4](../design/04-claim-lifecycle.md), [ADR-0011](0011-minimal-claim-data-model.md)

## Context

Bank Negara Malaysia's *Claims Settlement Practices* (section 10) and common insurance lifecycle descriptions go well
beyond the brief: statutory deadlines, a formal offer and acceptance, payment, and motor-specific rules. They are one
market's regulation and a source of realism, not requirements.

## Decision

The lifecycle is Submitted, UnderReview, InfoRequested, Settled and Rejected. These are left out on purpose:

- working-day deadlines, reminders and overdue tracking;
- separate offer, acceptance and payment steps, and a closed state;
- reopening, appeals and the Ombudsman notice on rejections;
- coverage verification and reserves;
- motor rules such as independent adjusters, second inspections and itemised repair estimates.

## Consequences

- A lifecycle small enough to implement, test and explain. `Settled` stands in for offer, acceptance and payment.
- **With more time:** due dates and an overdue flag on the officer queue (the deadlines are working days, motor or
  non-motor), then offer, acceptance and payment as states, then the motor rules.

## Alternatives considered

- **Model the BNM process in full:** realistic, but a regulation for one of six markets and far beyond the time.
- **Only deadlines:** cheap to add, but needs a working-day calendar and a decision on which market's rules apply.
