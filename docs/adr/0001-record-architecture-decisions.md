# ADR-0001: Record architecture decisions

- **Status:** Accepted
- **Date:** 2026-11-24
- **Deciders:** Project owner
- **Related:** —

## Context

The assessment values the reasoning behind design decisions as much as the code, and the panel will probe choices
in a walkthrough. Decisions need to be captured when they are made, with the reasoning still fresh.

## Decision

We will use RFCs (`docs/rfcs/`) to discuss larger proposals before building, and ADRs (`docs/adr/`) to record
each significant decision. ADRs are immutable once Accepted; changes are made by superseding ADRs.

## Consequences

- There is a lightweight, reviewable trail from question → options → decision.
- Writing an ADR adds a few minutes per decision; small, obvious choices are left out.
- The AI journal (`docs/ai-journal.md`) records *how* AI was used; ADRs record *what* was decided and why.

## Alternatives considered

- **Decisions only in the README / commit messages**: hard to find and easy to lose the alternatives.
- **Wiki outside the repo**: not versioned with the code, and the brief asks for docs alongside the code.
