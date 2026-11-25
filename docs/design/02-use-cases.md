# 2. Use cases

One use case per requirement in the [RTM](01-requirements-traceability-matrix.md), so the numbers line up (UC-01 ↔ R1).
Actors are the two user types in the brief's requirement paragraphs. Mermaid has no native use case diagram, so this is
a `flowchart` with actors on the outside and use cases inside the system boundary.

**Status:** all eight are implemented, in the API and in the SPA. The [RTM](01-requirements-traceability-matrix.md)
lists the endpoint, screen and tests for each.

| UC | Screen |
| --- | --- |
| UC-01 | `/my-claims/new` (Report an incident) |
| UC-02, UC-03, UC-04 | `/my-claims` and `/my-claims/{id}` (claim, answer form, decision), plus the notifications bell |
| UC-05 | `/work` (queue tabs) and **Pick up** on `/work/{id}` |
| UC-06, UC-07 | `/work/{id}` (Ask the claimant, Decision) |
| UC-08 | `/work/team` (team workload, and claimed exposure for D7) |

## Use case diagram

```mermaid
flowchart LR
    claimant(["Claimant"])
    staff(["Claims staff"])

    subgraph system["Claims platform"]
        uc1(["UC-01 Report an incident"])
        uc2(["UC-02 Track a claim"])
        uc3(["UC-03 Provide additional information"])
        uc4(["UC-04 Receive a decision"])
        uc5(["UC-05 Pick up an incoming claim"])
        uc6(["UC-06 Review and assess a claim"])
        uc7(["UC-07 Settle or reject a claim"])
        uc8(["UC-08 View team workload and performance"])
    end

    claimant --- uc1
    claimant --- uc2
    claimant --- uc3
    claimant --- uc4

    staff --- uc5
    staff --- uc6
    staff --- uc7
    staff --- uc8
```

## Use case list

| UC | Use case | Actor | Requirement | Brief wording |
| --- | --- | --- | --- | --- |
| UC-01 | Report an incident | Claimant | R1 | "report an incident" |
| UC-02 | Track a claim | Claimant | R2 | "track their claim" |
| UC-03 | Provide additional information | Claimant | R3 | "provide additional information when asked" |
| UC-04 | Receive a decision | Claimant | R4 | "receive decisions" |
| UC-05 | Pick up an incoming claim | Claims staff | R5 | "pick up incoming claims" |
| UC-06 | Review and assess a claim | Claims staff | R6 | "review and assess them" |
| UC-07 | Settle or reject a claim | Claims staff | R7 | "progress claims to settlement or rejection" |
| UC-08 | View team workload and performance | Claims staff | R8 | "see their team's workload and performance" |

## Relationships between use cases

Not shown in the diagram, to keep it to what the brief states. Dependencies noted in the RTM justifications:

- UC-03 happens "when asked", so it depends on something in UC-06 that asks the claimant.
- UC-04 delivers the outcome of UC-07.
- UC-05 to UC-07 act on claims created by UC-01.

They stay plain dependencies, not `include`/`extend` relationships: the claim lifecycle (doc 4) expresses them as
status transitions.

## Open

- Specifications per use case (main flow, alternatives, preconditions) were not written. The lifecycle (doc 4) holds
  the rules and the tests named after each requirement cover the flows, so they were not needed to build.
- No manager actor: team workload and performance sits under claims staff, as in the brief.
