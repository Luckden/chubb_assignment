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
  exposure view. (deferred by me due to time limitation and scope of assignment)

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

### 2026-11-25: ADR acceptance

- **Asked:** To mark the ADRs accepted.
- **Accepted:** ADR-0002, 0003, 0016, 0017, 0018 and 0019 (the ones left open by the RFC-0002 work).
- **Challenged:** —
- **Overrode:** —
- **Open:** ADR-0005 and ADR-0014 are still Proposed. ADR-0013 still needs the `C3_*` naming exception.

### 2026-11-25: Constraint-traced test naming (ADR-0020)

- **Asked:** To work on what was blocked on the ADR acceptance.
- **Accepted:** The AI's reading that this meant the ADR-0013 amendment for `C3_*` tests, written as ADR-0020
  (Accepted) because ADR-0013's text is frozen. The existing Kafka round-trip test is renamed to follow it.
- **Challenged:** —
- **Overrode:** —
- **Open:** Whether something else was meant. ADR-0005 and ADR-0014 are still Proposed.

### 2026-11-25: Closing RFC-0002 gaps

- **Asked:** For a decision to iterate on, then chose outbox lag logging and design doc 4 over the broker-down test,
  notification e2e and full-stack check, in one commit.
- **Accepted:** Lag logged by the relay (warning above 30 s); design doc 4's candidate events replaced by the
  catalogue.
- **Challenged:** —
- **Overrode:** —
- **Open:** Broker-down test, notification e2e and a full `docker compose` run are not done.

### 2026-11-25: Broker-down test

- **Asked:** The broker-down test, from the remaining RFC-0002 gaps.
- **Accepted:** A test with its own Postgres and Kafka containers that stops the broker, submits a claim (201), checks
  the event waits in the outbox, restarts the broker and checks the event reaches the topic.
- **Challenged:** —
- **Overrode:** —
- **Open:** Notification e2e and a full `docker compose` run are not done.

### 2026-11-25: Notification e2e

- **Asked:** The notification e2e spec, from the remaining RFC-0002 gaps.
- **Accepted:** Two Playwright specs with the API mocked: a claimant sees one unread notification, opens it, lands on
  the claim and the bell drops to zero; an officer has no bell.
- **Challenged:** —
- **Overrode:** —
- **Open:** A full `docker compose` run is not done. Something already listens on port 4200 and served an older build,
  so `npm run e2e` reused it and failed. I did not stop it.

### 2026-11-25: Full docker compose check

- **Asked:** The full `docker compose` run with Kafka auto-create off.
- **Accepted:** Rebuilding the existing stack (kafka, api, web recreated; volumes kept), which also tested the
  migrations on a database with data.
- **Challenged:** —
- **Overrode:** —
- **Open:** The first run failed: the naming convention renamed EF's migration-history columns, so the API could not
  start on an existing database. Fixed with a guarded one-off rename at startup, and ADR-0019 corrected. The real stack
  then migrated, created both topics, published all three events in order and produced the notification.

### 2026-11-25: Outbox clean-up

- **Asked:** Implement missing implementation for the one RFC-0002 item left, outbox clean-up.
- **Accepted:** Deleting published rows after 7 days, hourly, from the relay; unpublished rows are never deleted.
  The 7 days is the AI's default.
- **Challenged:** —
- **Overrode:** —
- **Open:** _

### 2026-11-25: UI for the backend changes, and RFC-0003 built

- **Asked:** To update the UI for the backend changes, check whether the API contract needed regenerating, and whether
  new pages were needed for liability exposure and work efficiency, looking at RFC-0002 and RFC-0003.
- **Accepted:**
  - RFC-0002's UI was already done and the contract was current, so nothing changed there.
  - RFC-0003's two open decisions: the performance figures (decisions in 30 days, settled vs rejected, average days
    to decision) and ADR-0014 (workload computed on read from the status history).
  - Building RFC-0003: workload and exposure endpoints, time in status on the claim list, a three-tab work queue and
    a staff Team page.
- **Challenged:** —
- **Overrode:** —

### 2026-11-25: Docs brought up to date

- **Asked:** To update all docs to match what is built.
- **Accepted:** The ERD now shows the notifications and outbox tables; the use cases list each screen; the lifecycle,
  scope, RFC and design index status lines say what is built; ADR-0005 marked Accepted, since I chose Angular
  Material earlier; the frontend README replaces the Angular CLI boilerplate; AGENTS.md describes the outbox flow and
  the `R`/`C`/`D` test name prefixes.
- **Challenged:** —
- **Overrode:** —
- **Open:** The RTM Priority column (Must / Should / Won't) is still mine to fill in.

### 2026-11-25: Generated Synthetic data for mocking

- **Asked:** Provided the assignment specs file, generate a synthetic dataset to test the system's capability.
- **Accepted:** One instructions file for my other agent to validatre the system along with and a synthetic dataset in csv.
- **Challenged:** —
- **Overrode:** —
- **Open:** _

### 2026-11-25: Synthetic claims data seeding

- **Asked:** Whether the synthetic APAC data set (zip plus README) could be used, then to map it into the current model.
- **Accepted:** Fitting the data to the four-table model, not extending the model: customers, staff, claims, history
  and information requests are loaded; policies, reserves, payments, documents and notes are dropped. Ten statuses
  collapse into five (a withdrawal becomes a rejection), amounts convert to USD, and loading is on in Development
  only (ADR-0021). The AI chose the status mapping and the USD conversion.
- **Challenged:** —
- **Overrode:** —
- **Open:** `invalid_claim_submissions.csv` as validation fixtures; policies and reserves as a later extension.

### 2026-11-25: Names on seeded claims

- **Asked:** Seeded claims showed "Unknown user", which is misleading once a claim is rejected. Assign a user called
  SampleClaimant, and sample staff.
- **Accepted:** —
- **Challenged:** —
- **Overrode:** —
- **Open:** —

### 2026-11-25: Remove the sample claimant

- **Asked:** Remove SampleClaimant and all of that user's claims. Too many claims are seeded for the brief's time limit, and the sample claimant is not in the brief.
- **Accepted:** —
- **Challenged:** —
- **Overrode:** —
- **Open:** —

### 2026-11-25: Requirement priorities

- **Asked:** All requirements are top priority, since all are requirements for the assignment.
- **Accepted:** —
- **Challenged:** —
- **Overrode:** —
- **Open:** —

### 2026-11-25: Seed data on a fresh clone

- **Asked:** A clone on another PC had no data after `docker compose up --build`.
- **Accepted:** —
- **Challenged:** —
- **Overrode:** —
- **Open:** —

### 2026-11-25: Main users and a short sample

- **Asked:** The data change was removal of the huge list, with sample users: the main users and some samples.
- **Accepted:** —
- **Challenged:** —
- **Overrode:** —
- **Open:** —
