# Documentation

| Folder / file | What goes there |
| --- | --- |
| [rfcs/](rfcs/README.md) | **RFCs**: proposals that get discussed *before* building (problem, options, recommendation). |
| [adr/](adr/README.md) | **ADRs**: short, immutable records of decisions that were made, and why. |
| [design/](design/README.md) | **Design documents**, numbered in the order they are written: requirements traceability matrix, use cases, ERD, claim lifecycle. |
| [scope-and-prioritisation.md](scope-and-prioritisation.md) | Objectives, deliverables, constraints and the brief's design questions. |
| [ai-journal.md](ai-journal.md) | Running log of AI-assisted work: what was asked, accepted, challenged and overridden. |

## Flow: RFC → ADR

```
 idea / question
       │
       ▼
 RFC (Draft) ──► discussion ──► RFC (Accepted | Rejected | Withdrawn)
                                      │
                                      ▼
                    one or more ADRs (Accepted), each linking back to the RFC
                                      │
                                      ▼
                    design/ docs updated to match
```

- **Write an RFC** when a change is cross-cutting, hard to reverse, or has several viable options worth comparing
  (service boundaries, sync vs async, data model, frontend architecture).
- **Write an ADR** for every significant decision, whether or not it came from an RFC. Small, obvious decisions
  can skip the RFC and go straight to an ADR.
- **ADRs are immutable** once Accepted. To change a decision, write a new ADR that *supersedes* the old one and update the old one's status line only.
- Keep both short. An RFC is roughly a page or two; an ADR is a few paragraphs.

Templates: [rfcs/0000-template.md](rfcs/0000-template.md), [adr/0000-template.md](adr/0000-template.md).
In Claude Code, `/new-rfc <title>` and `/new-adr <title>` create a numbered file from these templates.
