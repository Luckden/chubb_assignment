# Architecture Decision Records

Short records of significant decisions, in the style of Michael Nygard's ADRs.
See the [docs README](../README.md) for how ADRs relate to RFCs.

## Process

1. Copy [0000-template.md](0000-template.md) to `NNNN-short-title.md` (next free number, 4 digits, kebab-case).
2. Write it with status **Proposed**, link the RFC it came from (if any), and add it to the index below.
3. Once agreed, set it to **Accepted**. From then on the content does not change.
4. To reverse or change a decision, write a new ADR and mark the old one `Superseded by ADR-NNNN`.

Statuses: `Proposed` → `Accepted` → (`Deprecated` | `Superseded by ADR-NNNN`), or `Rejected`.

## Index

| # | Title | Status |
| --- | --- | --- |
| [0001](0001-record-architecture-decisions.md) | Record architecture decisions | Accepted |
| [0002](0002-baseline-technology-stack.md) | Baseline technology stack | Proposed |
| [0003](0003-engineering-guardrails-for-ai-assisted-development.md) | Engineering guardrails for AI-assisted development | Proposed |
| [0004](0004-frontend-ui-and-server-state-libraries.md) | Frontend UI and server-state libraries | Superseded by 0005 |
| [0005](0005-use-angular-material-for-ui-components.md) | Use Angular Material for UI components | Proposed |
| [0006](0006-backend-layered-structure.md) | Backend as one layered project with feature folders | Accepted |
| [0007](0007-application-services-and-ef-core-conventions.md) | Application services, no repositories, rich domain and EF Core conventions | Accepted |
| [0008](0008-authorisation-by-role-and-ownership-no-rebac.md) | Authorisation by role and ownership checks, no ReBAC | Accepted |
| [0009](0009-authentication-deferred.md) | Authentication deferred; requests name the acting user | Accepted |
| [0010](0010-information-requests-as-structured-questions-not-chat.md) | Information requests are structured questions, not chat or email | Accepted |
| [0011](0011-minimal-claim-data-model.md) | Minimal claim data model | Accepted |
| [0012](0012-claims-handling-rules-left-out.md) | Real-world claims handling rules are left out | Accepted |
| [0013](0013-test-conventions-and-single-test-project.md) | Test conventions and a single test project | Accepted |
| [0014](0014-workload-views-computed-on-read-from-status-history.md) | Workload views are computed on read from the status history | Proposed |
| [0015](0015-exposure-is-claimed-amount-on-open-claims.md) | Liability exposure is the claimed amount on open claims | Accepted |
