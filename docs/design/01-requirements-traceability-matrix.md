# 1. Requirements traceability matrix

A requirements traceability matrix (RTM) links each requirement to the design that covers it and to the tests that
prove it, so nothing is built without a reason and nothing asked for is left untested. The brief is deliberately
underspecified, so this is how I show what was asked, what I decided to build, and where it lives. The layout follows
[Notion's RTM guide](https://www.notion.com/blog/requirements-traceability-matrix) (requirement ID and description,
justification, test case ID, test status, notes), with design columns added because there is no separate design spec.
The document itself lives in this repo, not in Notion.

**Requirements come only from the two requirement paragraphs of the brief** (what claimants need, what claims staff
need). The context paragraph (motor and property claims, six markets, phone and email intake, shared inboxes, managers
wanting exposure) describes what data and pain points exist. It is not a source of requirements, so it has no rows
here.

**Traceability in both directions**
- **Forward:** requirement → design (RFC/ADR) → implementation (endpoint, event) → test case.
- **Backward:** every test case is named with its requirement ID (`R{id}_{Method}_{Scenario}_{ExpectedBehavior}`, e.g.
  `R6_RequestInformation_OtherOfficer_ThrowsNotPermitted`, following the .NET unit testing naming guidance), so any test traces back to a requirement. A test with no requirement ID is out of scope.

**Column key**
- **Priority:** Must / Should / Won't. Every requirement is Must: each one is required by the assignment.
- **Design:** the use case ([doc 2](02-use-cases.md)), entities ([doc 3](03-erd.md)) and lifecycle states ([doc 4](04-claim-lifecycle.md)) that cover the requirement, and later the RFC or ADR that decides how it is met.
- **Implementation:** the endpoint, event or screen that delivers it. Filled in as code lands.
- **Test status:** Not run / Pass / Fail. Add who and when in Notes if useful.

## Requirements

| Req ID | Requirement | Justification | Priority |
| --- | --- | --- | --- |
| R1 | Claimant reports an incident | Entry point for every claim; the first step of the lifecycle, and the source of the data R5–R8 work on | Must |
| R2 | Claimant tracks their claim | Claimant visibility into status; depends on the lifecycle states defined for R5–R7 | Must |
| R3 | Claimant provides additional information when asked | Closes the loop with staff review; depends on a way for staff to ask (R6) | Must |
| R4 | Claimant receives decisions | Outcome of R7, delivered back to the claimant | Must |
| R5 | Staff pick up incoming claims | Moves a claim from submitted (R1) to being worked | Must |
| R6 | Staff review and assess claims | Core staff task; may trigger the information loop in R3 | Must |
| R7 | Staff progress claims to settlement or rejection | Ends the lifecycle; produces the decision seen in R4 | Must |
| R8 | Staff see their team's workload and performance | Gives staff a view of the work in R5–R7 and how it is going | Must |

## Traceability

| Req ID | Design | Implementation | Test case ID | Test status | Notes |
| --- | --- | --- | --- | --- | --- |
| R1 | UC-01; Claim (ERD); Submitted (lifecycle) | `POST /api/claims`; `Claim.Submit`; screen `/my-claims/new` | `R1_*` in ClaimTests, ClaimsApiTests; ReportIncidentPage spec; claims e2e | Pass |  |
| R2 | UC-02; Claim, status history (ERD) | `GET /api/claims`, `GET /api/claims/{id}`; screens `/my-claims`, `/my-claims/{id}` | `R2_*` in ClaimTests, ClaimsApiTests; ClaimantClaimPage spec | Pass |  |
| R3 | UC-03; Information request (ERD); InfoRequested to UnderReview | `POST /api/claims/{id}/information-response`; screen `/my-claims/{id}` | `R3_*` in ClaimTests, ClaimsApiTests, EventFlowTests; ClaimantClaimPage spec | Pass | The claimant is notified when information is requested (ADR-0018). |
| R4 | UC-04; Claim decision fields (ERD); Settled, Rejected | Decision fields on `GET /api/claims/{id}`; screen `/my-claims/{id}` | `R4_*` in ClaimsApiTests; ClaimantClaimPage spec | Pass | Claimant sees the decision by tracking. Notified through the Kafka consumer: `GET /api/notifications`, toolbar bell; `R4_*` and `C3_*` in EventFlowTests, NotificationsMenu spec (ADR-0018). |
| R5 | UC-05; Claim.assignee (ERD); Submitted to UnderReview | `GET /api/claims?unassigned=true`, `POST /api/claims/{id}/pick-up`; screens `/work`, `/work/{id}` | `R5_*` in ClaimTests, ClaimsApiTests; StaffClaimPage spec; claims e2e | Pass | Concurrent pick-up: one 200, one 409. |
| R6 | UC-06; Information request (ERD); UnderReview, InfoRequested | `POST /api/claims/{id}/information-requests`; screen `/work/{id}` | `R6_*` in ClaimTests, ClaimsApiTests; StaffClaimPage spec | Pass | Assessing is a review activity; no stored amount. |
| R7 | UC-07; Claim decision fields (ERD); UnderReview to Settled or Rejected | `POST /api/claims/{id}/settle`, `.../reject`; screen `/work/{id}` | `R7_*` in ClaimTests, ClaimsApiTests; StaffClaimPage spec | Pass |  |
| R8 | UC-08; status history (ERD); RFC-0003, ADR-0014 | `GET /api/workload`; `statusChangedAt` and `sort` on `GET /api/claims`; screens `/work` (queue tabs), `/work/team` | `R8_*` and `R5_ListClaims_*` in TeamViewsApiTests; WorkQueuePage and TeamPage specs; team e2e | Pass | Performance = decisions in 30 days, settled vs rejected, average days to decision (owner confirmed). Claimed exposure (D7, ADR-0015) is on the same screen: `GET /api/exposure`, `D7_*` tests. |

Update this table as RFCs, code and tests land. If a requirement is dropped, mark it Won't in Priority and say why in
Notes.
