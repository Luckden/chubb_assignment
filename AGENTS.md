# Agent instructions (all AI tools)

Claims platform take-home: Angular SPA + .NET API + PostgreSQL + Kafka. Backend first; frontend if time permits.

## Layout

- `backend/` — .NET 10 solution `ClaimsPlatform.slnx`
  - `src/Claims.Api/` — ASP.NET Core API (`Controllers/`, `Application/`, `Domain/`, `Infrastructure/{Persistence,Messaging}`)
  - `tests/Claims.Api.Tests/` — xUnit + `WebApplicationFactory`
- `frontend/` — Angular 21 workspace. **Read `frontend/AGENTS.md` before touching frontend code.**
- `docs/` — RFCs, ADRs, architecture docs, AI journal. See `docs/README.md`.
- `docker-compose.yml` — postgres, kafka (KRaft), kafka-ui, api, web.

## Commands

| Task | Command |
| --- | --- |
| Infra only | `docker compose up -d postgres kafka kafka-ui` |
| Full stack | `docker compose up --build` |
| API | `dotnet run --project backend/src/Claims.Api` (http://localhost:5080) |
| API tests | `dotnet test backend` |
| SPA | `cd frontend && npm start` (http://localhost:4200, proxies `/api`) |
| SPA tests | `cd frontend && npm test` |
| E2E (API mocked) | `cd frontend && npm run e2e` (starts `ng serve` itself) |
| E2E (full stack) | `docker compose up -d`, then `E2E_BASE_URL=http://localhost:4200 npm run e2e` in `frontend/` |
| EF migration | `dotnet ef migrations add <Name> --project backend/src/Claims.Api` |

## Conventions

- **Backend:** C# 14, nullable on, primary constructors, `async` all the way, `ProblemDetails` for errors.
  Controllers stay thin; logic lives in `Application/`; entities and rules live in `Domain/` with no framework dependencies.
- **Messaging:** publish through `IEventPublisher`; topic names live in `Infrastructure/Messaging/Topics.cs`.
  Events are past-tense facts (`ClaimSubmitted`), keyed by aggregate id.
- **Frontend:** follow `frontend/AGENTS.md` (standalone, signals, OnPush, `inject()`, native control flow).
- **Tests:** add or adjust tests with every behaviour change.
  - E2E specs live in `frontend/e2e/*.e2e.ts`. Use role-based locators (`getByRole`), and mock the API with
    `page.route` by default. Tag a spec `@stack` only if it needs the real backend.
- **Docs:** a significant design decision needs an ADR (`docs/adr/`); a cross-cutting proposal starts as an RFC
  (`docs/rfcs/`). Use the templates and update the index tables.
- **No** Chubb branding, proprietary material or public hosting.

## AI journal

The brief requires `docs/ai-journal.md`. When you finish a meaningful chunk of work, propose a journal entry
(asked / accepted / challenged / overrode / why). Do not invent the human's opinions; leave `My notes` for them.
