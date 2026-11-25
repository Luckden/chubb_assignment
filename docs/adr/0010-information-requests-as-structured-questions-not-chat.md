# ADR-0010: Information requests are structured questions, not chat or email

- **Status:** Accepted
- **Date:** 2026-11-25
- **Deciders:** Project owner
- **Related:** [Design doc 3](../design/03-erd.md), [Design doc 4](../design/04-claim-lifecycle.md), [RFC-0002](../rfcs/0002-sync-vs-async-communication.md)

## Context

R3 says the claimant provides additional information "when asked". That is a question and an answer, not a
conversation. Chat or email would be a feature beyond the brief.

## Decision

- An `InformationRequest` row holds one question from the assigned officer and the claimant's answer.
- A claim has one open request at a time. Asking moves it to InfoRequested; answering returns it to UnderReview.
  Several rounds are allowed, one after another.
- No chat, no email in or out, no real-time push. The claimant sees the question by tracking the claim.

## Consequences

- Small: one table and two endpoints, fully covered by tests.
- The claimant finds out about a question only by looking (or polling). Notifications depend on RFC-0002.
- **With more time:** a per-claim message thread, an email or notification consumer reading the Kafka events, and
  SignalR for live updates.

## Alternatives considered

- **Fields on Claim (one round):** fewer tables, but a second round overwrites the first.
- **Chat with SignalR:** conversational, but needs message storage, unread state and real-time infrastructure.
- **Email:** the brief names email as the current problem, and inbound parsing is hard.
