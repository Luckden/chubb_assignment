# Claims Platform

Take-home assessment: a claims platform for claimants and claims staff.
Angular SPA, .NET API, PostgreSQL and Kafka, run locally with Docker Compose.

> Status: **all eight requirements (R1–R8) and all seven design questions (D1–D7) are built**, in the API and in the
> SPA. Requirements are traced to code and tests in the
> [RTM](docs/design/01-requirements-traceability-matrix.md); decisions are in [RFCs](docs/rfcs/README.md) and
> [ADRs](docs/adr/README.md), including what was left out on purpose.

## What it does

| Who | Can | Requirement |
| --- | --- | --- |
| Claimant | Report an incident, see their claims and each claim's history | R1, R2 |
| Claimant | Answer a question from the officer, see the decision and its reason, get a notification for both | R3, R4 |
| Claims staff | Work a queue (needs me, waiting on claimant, unassigned), oldest in status first; pick up a claim | R5 |
| Claims staff | Ask the claimant for information, then settle or reject (a rejection needs a reason) | R6, R7 |
| Claims staff | See the team's workload and recent decisions, and the claimed exposure on open claims | R8, D6, D7 |

**How it fits together** (RFC-0001, RFC-0002): one ASP.NET Core API over PostgreSQL. Every user action is a
synchronous REST call, so the caller gets its 400/403/409 straight away. Each claim transition also writes a
past-tense event (`ClaimSubmitted`, `ClaimPickedUp`, `InformationRequested`, `InformationProvided`, `ClaimSettled`,
`ClaimRejected`) to a transactional outbox in the same database transaction. A background relay publishes it to Kafka
(`claims.claim-events`, keyed by claim id), and a consumer turns events into claimant notifications. A Kafka outage
never fails or loses a REST call. The SPA picks up changes by polling every 10 seconds (ADR-0017).

**Left out on purpose** (each has an ADR): authentication (ADR-0009), fine-grained authorisation (ADR-0008), chat
or email threads (ADR-0010), policies, reserves, currencies and attachments (ADR-0011), and regulatory deadlines
(ADR-0012). Claimed exposure is an upper bound, not a reserve (ADR-0015).

## Prerequisites

| Tool | Version |
| --- | --- |
| .NET SDK | 10.0.x (LTS), pinned in [backend/global.json](backend/global.json) |
| Node.js | 24 LTS ([.nvmrc](frontend/.nvmrc)) |
| Docker Desktop | with Compose v2+ |

## Run

Step-by-step for a fresh machine, with troubleshooting and how to stop it: **[docs/run-guide.md](docs/run-guide.md)**.

**Everything in containers**

```bash
docker compose up --build
```

**Infrastructure in containers, app from the IDE** (faster inner loop)

```bash
docker compose up -d postgres kafka kafka-ui
dotnet run --project backend/src/Claims.Api      # http://localhost:5080
cd frontend && npm ci && npm start               # http://localhost:4200
```

| URL | What |
| --- | --- |
| http://localhost:4200 | Angular SPA (see [Use the app](#use-the-app)) |
| http://localhost:5080/api/claims | Claims API (see [Try the API](#try-the-api)) |
| http://localhost:5080/api/system/info | Service information |
| http://localhost:5080/health/live, /health/ready | Liveness, and readiness (checks Postgres) |
| http://localhost:5080/openapi/v1.json | OpenAPI document (Development only) |
| http://localhost:8081 | Kafka UI |

Override ports and credentials by copying [.env.example](.env.example) to `.env`.

## Use the app

There is no sign-in. Pick who you are with **Acting as** in the toolbar; it switches between the claimant and staff
screens.

1. As **Alice Tan**, choose **Report an incident**, fill it in and submit. The claim opens with its history.
2. Switch to **Carol Wong**. The claim is under **Work queue → Unassigned**. Open it and choose **Pick up**.
3. Under **Ask the claimant**, send a question. The claim moves to **Waiting on claimant**.
4. Switch back to Alice. The bell shows a notification; open it and answer the question.
5. As Carol again, under **Decision**, settle or reject. Alice sees the decision and gets a notification.
6. As Carol, open **Team** for the claimed exposure and the team's workload and decisions.

Changes made as one user show up for the other within 10 seconds, without a reload.

## Try the API

There is no authentication yet. Each request names the acting user, and the API checks that the user exists and has
the right role. Four demo users are seeded on startup (Development and tests):

| User | Role | Id |
| --- | --- | --- |
| Alice | Claimant | `00000000-0000-7000-8000-000000000001` |
| Bob | Claimant | `00000000-0000-7000-8000-000000000002` |
| Carol | Officer | `00000000-0000-7000-8000-000000000011` |
| Dave | Officer | `00000000-0000-7000-8000-000000000012` |

| Method and path | What | Requirement |
| --- | --- | --- |
| `POST /api/claims` | Report an incident (claimant) | R1 |
| `GET /api/claims?claimantId=&assigneeId=&status=&unassigned=&sort=` | List claims, oldest first by `SubmittedAt` (default) or `StatusChangedAt` | R2, R5 |
| `GET /api/claims/{id}` | Track a claim: status, history, information requests, decision | R2, R4 |
| `POST /api/claims/{id}/pick-up` | Officer picks up a claim | R5 |
| `POST /api/claims/{id}/information-requests` | Assigned officer asks the claimant a question | R6 |
| `POST /api/claims/{id}/information-response` | Claimant answers the open question | R3 |
| `POST /api/claims/{id}/settle` | Assigned officer settles (reason optional) | R7 |
| `POST /api/claims/{id}/reject` | Assigned officer rejects (reason required) | R7 |
| `GET /api/notifications?userId=` | A user's notifications, newest first | R3, R4 |
| `POST /api/notifications/{id}/read` | Mark a notification read | R3, R4 |
| `GET /api/workload?officerId=` | The officer's team: open claims and decisions in the last 30 days per officer | R8 |
| `GET /api/exposure?officerId=` | Claimed exposure on open claims, by status, type and age | D7 |

Errors are `ProblemDetails`: 400 validation, 403 wrong user or role, 404 unknown claim, 409 illegal state change or a
concurrent update. The lifecycle is in [docs/design/04-claim-lifecycle.md](docs/design/04-claim-lifecycle.md).

```bash
curl -X POST http://localhost:5080/api/claims -H "Content-Type: application/json" -d '{
  "claimantId": "00000000-0000-7000-8000-000000000001", "type": "Motor", "incidentDate": "2026-11-20",
  "description": "Rear-ended at a junction", "claimedAmount": 1200 }'
```

## Test

```bash
dotnet test backend              # integration tests; Testcontainers starts Postgres + Kafka (Docker required)
./scripts/test-backend-in-docker.sh   # same, inside the SDK container (see Troubleshooting)
cd frontend && npm run lint && npm run format:check
cd frontend && npm test          # unit (Vitest)
cd frontend && npm run e2e       # end-to-end (Playwright), API mocked; starts ng serve itself
```

The first time, run `npx playwright install chromium` in `frontend/` to download the browser.
To run the end-to-end tests against the real stack, including specs tagged `@stack`, start `docker compose up -d` and then run
`E2E_BASE_URL=http://localhost:4200 npm run e2e`. Use `npm run e2e:report` to open the last HTML report.

CI ([.github/workflows/ci.yml](.github/workflows/ci.yml)) runs all of the above on every PR, and also checks that the
OpenAPI contract and the generated frontend types are up to date.

## API contract

The backend owns the contract. Building the API writes `backend/openapi/claims-api.json`, and
`cd frontend && npm run api:generate` turns it into TypeScript types in `frontend/src/app/core/api/generated/`.
Commit both after any API change.

## Repository layout

```
backend/      .NET 10 solution (Claims.Api + tests)
frontend/     Angular 21 workspace (claims-web)
docs/         RFCs, ADRs, design docs (RTM, use cases, ERD, lifecycle), AI journal
AGENTS.md     Instructions for AI coding agents (CLAUDE.md imports it)
.mcp.json     MCP servers (Angular CLI, Playwright) for Claude Code
```

## Documentation

See [docs/README.md](docs/README.md) for the RFC → ADR flow. The AI working journal is in
[docs/ai-journal.md](docs/ai-journal.md).

## Troubleshooting (Windows)

- **`dotnet test`: "An Application Control policy has blocked this file"**: Windows Smart App Control / WDAC is
  blocking the locally built test DLL. Use `./scripts/test-backend-in-docker.sh` (Git Bash), which runs the suite in
  the .NET SDK container.
- **`npm test`: "Vitest failed to find the runner"**: the shell's working directory has a lowercase drive letter
  (`c:\…`). Open a new terminal at `C:\…` and run it again.
- **`npm run e2e` tests an old build**: Playwright reuses whatever already listens on port 4200. If the `web`
  container is running, rebuild it first (`docker compose up -d --build web`) or stop it so `ng serve` starts.
