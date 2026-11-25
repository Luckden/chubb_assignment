# ADR-0021: Load the synthetic claims data into the current model

- **Status:** Accepted
- **Date:** 2026-11-25
- **Deciders:** Project owner
- **Related:** [ADR-0011](0011-minimal-claim-data-model.md), [ADR-0015](0015-exposure-is-claimed-amount-on-open-claims.md), [ADR-0016](0016-rest-for-commands-kafka-for-lifecycle-events-via-outbox.md)

## Context

A synthetic APAC motor and property data set (six markets, about 1,400 claims) was offered for development and
evaluation. It models far more than the API does: policies, markets and currencies, reserves, payments, documents,
notes, ten claim statuses, and string ids such as `CLM-001077`. ADR-0011 deliberately keeps the model to four tables,
and scope decisions are the project owner's. The owner chose to fit the data to the current model, not to extend the
model to fit the data.

## Decision

We will load the data through an opt-in seeder, `SampleDataSeeder`, mapping it onto the existing entities:

- **Loaded:** claims, status history and information requests. The six CSV files are embedded in the API assembly.
  The 900 customers and 68 officers are not separate accounts. Every sample claimant is the user SampleClaimant, and
  every sample officer — including whoever rejected or settled a claim — is Sample Staff (`team` `sample`).
- **Not loaded:** policies, vehicles and properties, reserves, payments, documents, notes, teams and the test fixtures
  (`edge_cases.csv`, `invalid_claim_submissions.csv`). The model has nowhere to put them (ADR-0011).
- **Statuses:** `SUBMITTED` stays; `ASSIGNED`, `UNDER_REVIEW` and `REOPENED` become `UnderReview`; `AWAITING_INFO`
  becomes `InfoRequested`; `APPROVED`, `SETTLED` and `CLOSED` become `Settled`; `REJECTED` and `WITHDRAWN` become
  `Rejected`. A withdrawal has no equivalent, so it is `Rejected` with the reason "Withdrawn by the claimant".
  History events that leave the mapped status unchanged add no row.
- **Amounts:** `estimated_loss` converts to USD with `markets.fx_rate_to_usd`, because the model has no currency and
  exposure (ADR-0015) sums `claimed_amount` across markets.
- **Ids:** each source id becomes a deterministic GUID (SHA-256 of the source id), so repeated loads find the same rows.
  System events (the payments service) are attributed to the claim's officer.
- **Events:** loading goes through `Claim.Restore`, which checks no lifecycle rules and raises no events. The facts
  predate the system, so nothing reaches the outbox, Kafka or the notifications table.
- **Switch:** `Database:SeedSampleData`, on in `appsettings.Development.json` (so Docker Compose and `dotnet run`).
  Loading skips rows already present. Integration tests use the Testing environment, which leaves it off.

## Consequences

- The work queue, workload and exposure views have realistic volume. Open exposure is about USD 1.68M, close to the
  data set's own figure of USD 1.6M, which also counts reserves.
- The mapping loses information: a reopened claim has lost its "reopened" marker, and the deductible and sum insured
  checks the data set was designed for (`edge_cases.csv`) cannot be exercised.
- Acting as keeps Alice, Bob, Carol and Dave, and adds SampleClaimant and Sample Staff. A sample claim names one of
  those two. Sample Staff's team is `sample`, so Carol and Dave's team is unchanged. Alice's own list stays empty
  until she reports an incident.
- **With more time:** policies and reserves (ADR-0011's follow-up), which would make the dropped files usable, and
  `invalid_claim_submissions.csv` as validation test fixtures.

## Alternatives considered

- **Extend the domain to the full data set:** realistic, but a feature-sized change that no requirement asks for.
- **Test fixtures only:** no schema change, but the UI stays empty.
- **Always on:** the integration tests count claims and exposure, so a loaded database would break them.
