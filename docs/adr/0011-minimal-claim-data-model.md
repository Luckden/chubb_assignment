# ADR-0011: Minimal claim data model

- **Status:** Accepted
- **Date:** 2026-11-25
- **Deciders:** Project owner
- **Related:** [Design doc 3](../design/03-erd.md), [Design doc 1](../design/01-requirements-traceability-matrix.md)

## Context

Requirements come from the two requirement paragraphs of the brief (R1 to R8). The context paragraph (motor and
property, six markets, phone and email intake) describes data that exists, but does not ask for it to be modelled.

## Decision

Model only what R1 to R8 need: User, Claim (with the decision as fields), ClaimStatusChange and InformationRequest, with
`claimed_amount` as the only amount. Not modelled:

- policies, coverage and insured items;
- attachments such as photos and receipts;
- markets and currency;
- reserves, payments and an assessed amount;
- a separate Decision entity and a Team entity (`team_id` is a field on User).

## Consequences

- Four tables, easy to explain and test.
- Coverage cannot be verified, and liability exposure (D7) can only be a sum of `claimed_amount`.
- **With more time:** a Policy entity with coverage rules, attachments, market and currency, and reserves and payments
  for a real exposure figure.

## Alternatives considered

- **Policy and insured item entities:** realistic, but a lot of modelling that no requirement asks for.
- **Market and currency fields:** two fields, but nothing in R1 to R8 uses them.
- **Assessed amount or reserves:** an amount the officer sets while assessing is not in the brief.
