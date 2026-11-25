# AI working journal

A running log of how I used AI on this assignment: what I asked for, what I accepted, and where I challenged or
overrode it. Kept short and in my own terms. Newest entries at the bottom.

**Tools:** Claude Code (Claude Opus 5.5) in VS Code, with the Angular CLI, Playwright and daisyUI MCP servers and the
daisyUI skill.

## Entry template

```
### YYYY-MM-DD: <topic>
- **Asked:**
- **Accepted:**
- **Challenged:**
- **Overrode:**
```

---

### 2026-11-24: Project scaffolding

- **Asked:** An empty skeleton: .NET Web API and Angular on their latest LTS versions, Node, Kafka, PostgreSQL,
  Docker Compose and docs, with no domain code. While it was running I added two more requests: my usual RFC + ADR
  documentation flow, and AI instruction files and MCP servers (Angular's AI setup).
- **Accepted:** .NET 10, Angular 21, Node 24, Postgres 18, Kafka 4.1 (KRaft) with Kafka UI, a Compose stack, an
  RFC/ADR structure with templates, `AGENTS.md`/`CLAUDE.md`, and the Angular CLI MCP server.
- **Challenged:** —
- **Overrode:** —

### 2026-11-24: Playwright

- **Asked:** Add Playwright.
- **Accepted:** E2E tests that mock the API by default, plus one full-stack smoke test, and the Playwright MCP server.
- **Challenged:** —
- **Overrode:** —

### 2026-11-25: Gap analysis and guardrails

- **Asked:** What documentation and tech stack were missing against the brief. After the AI's list, I told it to go
  ahead, and explained that the goal is a setup that makes later AI-assisted coding more reliable.
- **Accepted:** Stricter backend build, real-infrastructure integration tests (Testcontainers), OpenAPI-generated
  frontend types, lint and format, GitHub Actions CI, a scope doc, and RFC-0001/0002 as question-only drafts.
- **Challenged:** —
- **Overrode:** —
- **Deferred:** Auth, real-time updates, outbox, file uploads, observability.

### 2026-11-25: Frontend UI libraries

- **Asked:** As someone new to Angular, whether I was missing UI libraries like daisyUI or TanStack Query. Then I
  pointed the AI at daisyUI's Claude Code skill page so that it can setup skills.
- **Accepted:** Tailwind + daisyUI, Angular CDK, TanStack Query and lucide icons (ADR-0004), plus the daisyUI skill and
  the free daisyUI MCP server.
- **Challenged:** —
- **Overrode:** I chose libraries I already know from React (daisyUI, TanStack Query), even though Angular Material
  is the standard Angular choice.

### 2026-11-25: Deciding which design documents to produce

- **Asked:** Whether the actor diagram covers the full requirements, and whether to add ReBAC or authorization
  policies. Which parts of the brief were still missing. Whether to use use-case diagrams, an ERD and a requirements
  traceability matrix (RTM), which I raised because of how this assignment is structured (an underspecified brief
  where prioritisation and explanation are assessed)
  - The design set: RTM, use cases, ERD, claim lifecycle state diagram, assumptions and open questions, and service
    boundaries. Diagrams are written as Mermaid in markdown.
  - No ReBAC. Auth is still undecided.
- **Challenged:**
  - The AI's suggestion of authorization policies, as too much for the time available. It withdrew the suggestion.
  - The AI saying it hadn't seen the brief, when the brief was in `docs/`.
- **Overrode:** The AI's proposal to make managers a separate actor. The brief's staff requirements already cover insights to 
  team workload.
- **Still open:** Policy, Decision, InformationRequest, attachments, Team, Market/Currency, the officer view, and the
  exposure view.

### 2026-11-25: Switch to Angular Material

- **Asked:** Use Angular Material instead of daisyUI as it is more native.
- **Accepted:** Material 3 theme with light and dark modes, Material Symbols icons, and Tailwind kept for layout only
  (ADR-0005, which supersedes ADR-0004's UI choice).
- **Challenged:** —
- **Overrode:** The AI's earlier recommendation of daisyUI and its skill.

### 2026-11-25: Design docs and backend architecture

- **Asked:** For a requirements traceability matrix (RTM), using Notion's guide only as a reference, then use cases,
  an ERD and a claim lifecycle, as numbered design docs in `docs/design/`. Whether to base the design on the BNM
  claims settlement PDF, whether my lifecycle matches typical insurance sources, and whether the design is too heavy
  for the time. Then how to structure the backend (DDD, three-layer or event-driven), and an RFC and ADR for the .NET
  Web API with EF Core.
- **Accepted:**
  - Design docs 1 to 4, with the ERD decisions: information request as its own table, status history kept, and
    `claimed_amount` as the only amount field.
  - The BNM PDF and the online sources as references only. What the design leaves out is listed in doc 4.
  - Shrinking the design docs and writing no more of them.
  - One layered project with feature folders inside `Application/` (ADR-0006), and RFC-0001 renumbered to cover the
    backend architecture.
- **Challenged:**
  - Whether the information request needs its own store and whether the brief asks for auditing. The AI agreed the
    brief doesn't.
  - `assessed_amount`, as weird. Removed.
  - Which open decisions the brief doesn't need. The AI removed them.
- **Overrode:**
  - The AI's matrix rows taken from the brief's context paragraph. Requirements come only from the two requirement
    paragraphs.
  - The sections the AI added to the scope doc (in scope, acceptance criteria, assumptions, risks, time budget).
    Removed, and the RTM moved out into `docs/design/`.


### 2026-11-25: Backend implementation, tests and tooling

- **Asked:** To implement what RFC-0001 and the ADRs planned (domain, EF Core, services, controllers). How the tests
  run (`dotnet test`, `dotnet run` or `dotnet build`), and for the domain tests to be run and extended. Whether
  docker compose is wired for all the infrastructure. Whether to switch to minimal APIs, and whether to use
  `dotnet/skills`. To update the docs to match the code. Whether my tests follow Microsoft's unit testing guidance.
  Whether the decisions are recorded, for the journal to always be updated, and for each deliberate omission to get
  its own ADR.
- **Accepted:**
  - Backend steps 1 to 4 for R1–R7, and RFC-0001, ADR-0006 and ADR-0007 as Accepted.
  - Controllers, not minimal APIs. The alternative is a row in RFC-0001.
  - Tests named `R{requirement}_{Method}_{Scenario}_{ExpectedBehavior}`, with one act each and shared constants.
  - Running the tests in Docker because the host blocks the test DLL.
  - One ADR per deliberate omission (ADR-0008 to ADR-0013: no ReBAC, authentication deferred, information requests
    not chat, minimal data model, claims handling rules left out, test conventions). All accepted.
- **Challenged:**
  - Whether the tests follow Microsoft's guidance. The AI found five gaps: naming, a separate unit-test project,
    several acts per test, logic in tests, and magic values.
  - Whether decisions are recorded. The AI found the journal had no entry for this work and that the deliberate
    omissions (ReBAC, chat, auth, outbox, attachments, BNM deadlines) were only in the chat.
- **Overrode:** The AI's reading of "implement" as adding the minimal API row. I meant the plan in the RFC and ADRs.
- **Open:** Whether to use `dotnet/skills` (nothing installed). The unit and integration test projects are not split
  yet.

### 2026-11-25: Audit of RFC-0002 (Kafka) before implementation

- **Asked:** To audit RFC-0002 and the related ADRs before any Kafka work starts, then to draft the proposal.
- **Accepted:** Claimant notifications as the Kafka consumer, although the brief doesn't name them, over a smaller
  audit-log consumer. The rest of the draft (REST for commands and queries, lifecycle events on one topic through a
  transactional outbox, polling in the frontend) is not reviewed yet.
- **Challenged:** —
- **Overrode:** —
- **Open:** Reviewing the rest of the draft. ADR-0002 and ADR-0003 are still Proposed, and the outbox omission discussed earlier had no ADR.

### 2026-11-25: Officer workload (D6) and liability exposure (D7)

- **Asked:** Which of the brief's design questions are still uncovered. Whether D6 and D7 are analytics, and whether
  D6 is mostly UI (the answer: D6 is the officer's own queue plus R8's team analytics, and needs time in status from
  the backend). Then an RFC and ADRs for D6 and D7 as the next work. An explanation of the exposure decision.
- **Accepted:** "Claimed exposure" for D7: the sum of `claimed_amount` on open claims, labelled as an upper bound,
  over adding a reserve now (which would reverse ADR-0011 and cost build time). Officers see the exposure view; no
  Manager role is added. ADR-0015 accepted. The rest of RFC-0003 and ADR-0014 is not reviewed yet.
- **Challenged:** —
- **Overrode:** —
- **Open:** Which performance figures R8 shows. Whether officers, with no Manager role, see the exposure view.

### 2026-11-25: Implementing RFC-0002 (Kafka events, outbox, notifications, polling)

- **Asked:** Whether RFC-0002 was implemented (it was not), then to implement it.
- **Accepted:** The RFC as drafted: lifecycle events through a transactional outbox, one in-process consumer building
  claimant notifications, frontend polling every 10 s. ADR-0016, 0017 and 0018 are written as Proposed.
- **Challenged:** —
- **Overrode:** —
- **Open:** Accepting ADR-0016 to 0018, and ADR-0002 and ADR-0003 (still Proposed, and this work builds on both).
  ADR-0013 still needs the `C3_*` constraint-traced naming exception. No test stops the broker to prove REST keeps
  working. Not committed yet.

### 2026-11-25: snake_case column names

- **Asked:** Whether SQL convention makes tables snake_case. Columns were still PascalCase, so then to continue.
- **Accepted:** Renaming all columns to snake_case with a naming-convention package and a rename migration (ADR-0019,
  Proposed).
- **Challenged:** That the schema followed SQL convention. Only the table names did.
- **Overrode:** —
- **Open:** Accepting ADR-0019.
