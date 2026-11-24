---
description: Create a new RFC from the template
argument-hint: <title>
---

Create a new RFC titled "$ARGUMENTS".

1. Find the highest existing number in `docs/rfcs/` (ignore `0000-template.md`) and use the next one, zero-padded to 4 digits.
2. Copy `docs/rfcs/0000-template.md` to `docs/rfcs/NNNN-<kebab-case-title>.md`, set the title, status `Draft`,
   and today's date for Created/Updated.
3. Add a row to the index table in `docs/rfcs/README.md`.
4. Ask me for the problem statement and options before filling in the body. Do not invent requirements.
