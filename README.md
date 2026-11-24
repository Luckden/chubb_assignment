# Claims Platform

Take-home assessment: a claims platform for claimants and claims staff.
Angular SPA, .NET API, PostgreSQL and Kafka, run locally with Docker Compose.

> Status: **scaffold only**. No domain features yet. Decisions are tracked in [docs/adr](docs/adr/README.md).

## Prerequisites

| Tool | Version |
| --- | --- |
| .NET SDK | 10.0.x (LTS), pinned in [backend/global.json](backend/global.json) |
| Node.js | 24 LTS ([.nvmrc](frontend/.nvmrc)) |
| Docker Desktop | with Compose v2+ |

## Run

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
| http://localhost:4200 | Angular SPA (shows API connectivity) |
| http://localhost:5080/api/system/info | Placeholder API endpoint |
| http://localhost:5080/health/live, /health/ready | Liveness, and readiness (checks Postgres) |
| http://localhost:5080/openapi/v1.json | OpenAPI document (Development only) |
| http://localhost:8081 | Kafka UI |

Override ports and credentials by copying [.env.example](.env.example) to `.env`.

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
docs/         RFCs, ADRs, architecture, AI journal
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
