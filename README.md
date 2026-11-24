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
dotnet test backend
cd frontend && npm test          # unit (Vitest)
cd frontend && npm run e2e       # end-to-end (Playwright), API mocked; starts ng serve itself
```

The first time, run `npx playwright install chromium` in `frontend/` to download the browser.
To run the end-to-end tests against the real stack, including specs tagged `@stack`, start `docker compose up -d` and then run
`E2E_BASE_URL=http://localhost:4200 npm run e2e`. Use `npm run e2e:report` to open the last HTML report.

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
  blocking the locally built test DLL. Run the tests in a container or on a machine without that policy, or turn
  Smart App Control off.
- **`npm test`: "Vitest failed to find the runner"**: the shell's working directory has a lowercase drive letter
  (`c:\…`). Open a new terminal at `C:\…` and run it again.
