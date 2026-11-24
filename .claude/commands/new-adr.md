---
description: Create a new ADR from the template
argument-hint: <title>
---

Create a new ADR titled "$ARGUMENTS".

1. Find the highest existing number in `docs/adr/` (ignore `0000-template.md`) and use the next one, zero-padded to 4 digits.
2. Copy `docs/adr/0000-template.md` to `docs/adr/NNNN-<kebab-case-title>.md`, set the title, status `Proposed`, and today's date.
3. If it comes from an RFC, link it under **Related** and add the ADR to that RFC's *Outcome* section.
4. If it supersedes an ADR, change only the old ADR's status line to `Superseded by ADR-NNNN`.
5. Add a row to the index table in `docs/adr/README.md`.
6. Draft Context / Decision / Consequences from our conversation so far, and flag anything you are unsure of.
